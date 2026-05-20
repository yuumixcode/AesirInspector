import os
import sys
import json
import subprocess
import argparse
import tempfile
import requests
import re
import time
from jinja2 import Template


def get_pr_diff_via_api(owner_repo, pr_number, github_token):
    """通过 GitHub API 获取 PR 差异"""
    if not all([owner_repo, pr_number, github_token]):
        print("缺少必要参数，使用mock数据进行本地测试")
        return get_mock_diff_data()

    api_url = f"https://api.github.com/repos/{owner_repo}/pulls/{pr_number}"

    headers = {
        "Accept": "application/vnd.github.v3.diff",
        "Authorization": f"token {github_token}",
        "X-GitHub-Api-Version": "2022-11-28"
    }

    try:
        response = requests.get(api_url, headers=headers, timeout=30)
        if response.status_code == 200:
            return response.text
        else:
            print(f"获取PR差异失败，状态码: {response.status_code}, 响应内容: {response.text}")
            print("请检查仓库 Settings → Secrets and variables → Actions 是否配置了GITHUB_TOKEN，如已配置，请检查权限是否足够。")
    except requests.exceptions.RequestException as e:
        print(f"API请求失败: {str(e)}")
        print("使用mock数据进行本地测试")
        return get_mock_diff_data()

    return ""


def get_mock_diff_data():
    """返回模拟的diff数据用于本地测试"""
    mock_changes = [
        {
            "old_path": "Runtime/Unity/Core/AesirInspectorVersion.cs",
            "new_path": "Runtime/Unity/Core/AesirInspectorVersion.cs",
            "diff": """@@ -1,10 +1,15 @@
 namespace RunLab.AesirInspector
 {
     public static class AesirInspectorVersion
     {
-        public const string Value = "0.3.0";
+        public const string Value = "0.4.0-pre.1";
     }
 }
"""
        },
        {
            "old_path": "Runtime/Unity/Inspector/InspectorModel.cs",
            "new_path": "Runtime/Unity/Inspector/InspectorModel.cs",
            "diff": """@@ -15,8 +15,12 @@
 namespace RunLab.AesirInspector.Inspector
 {
     public class InspectorModel
     {
-        public string Title { get; set; }
-        public string? Description { get; set; }
+        public string Title { get; set; }
+        [SerializeField] private string description;
+
+        public void Reset()
+        {
+            description = null;
+        }
     }
 }
"""
        }
    ]
    return mock_changes


def filter_diff(diff_content):
    """过滤掉二进制文件、.meta 文件和第三方库"""
    binary_extensions = r"\.(png|jpg|jpeg|gif|svg|pdf|zip|tar|gz|jar|war|ear|class|so|dylib|dll|exe|bin|meta)$"

    if isinstance(diff_content, list):
        filtered_changes = []
        for change in diff_content:
            file_path = change.get('new_path', '') or change.get('old_path', '')
            if re.search(binary_extensions, file_path, re.IGNORECASE):
                continue
            diff = change.get('diff')
            if diff and isinstance(diff, str):
                filtered_changes.append(diff)
        return '\n'.join(filtered_changes)

    if isinstance(diff_content, str):
        filtered_lines = []
        in_binary_file = False
        for line in diff_content.split('\n'):
            if line.startswith('diff --git'):
                in_binary_file = False
                match = re.search(r'b/(.+)$', line)
                if match and re.search(binary_extensions, match.group(1), re.IGNORECASE):
                    in_binary_file = True
                    continue
            if not in_binary_file:
                filtered_lines.append(line)
        return '\n'.join(filtered_lines)

    return str(diff_content)


def gen_prompt(diff_content):
    """生成代码审查提示词"""
    script_dir = os.path.dirname(os.path.abspath(__file__))
    prompt_file = os.path.join(script_dir, 'codereview_prompt.md')

    try:
        with open(prompt_file, 'r', encoding='utf-8') as f:
            template = Template(f.read())
        return template.render(diff_content=diff_content)
    except FileNotFoundError:
        raise FileNotFoundError(f"Prompt file not found: {prompt_file}")
    except Exception as e:
        raise RuntimeError(f"Error reading prompt file: {e}")


def check_codely_installed():
    """检查codely是否已安装"""
    try:
        result = subprocess.run(
            ['codely', '--version'],
            capture_output=True, text=True, encoding='utf-8', errors='ignore'
        )
        return result.returncode == 0
    except FileNotFoundError:
        return False
    except Exception as e:
        print(f"检查codely安装状态时出错: {e}")
        return False


def call_codely_for_review(diff_content, codely_token):
    """使用codely进行代码审查"""
    request_start_time = time.time()

    prompt = gen_prompt(diff_content)

    tmp_path = None
    try:
        with tempfile.NamedTemporaryFile(mode='w', suffix='.md', delete=False, encoding='utf-8') as f:
            tmp_path = f.name
            f.write(prompt)

        codely_cmd = ['codely', '-p', f'@{tmp_path}', '-y']

        env = os.environ.copy()
        if codely_token:
            env['CODELY_TOKEN'] = codely_token

        process = subprocess.run(
            codely_cmd, shell=False, check=False, text=True,
            stdout=subprocess.PIPE, stderr=subprocess.PIPE, encoding='utf-8', errors='ignore',
            env=env
        )

        request_end_time = time.time()
        request_duration_seconds = round(request_end_time - request_start_time, 3)

        info = {
            "model_name": "codely",
            "token_info": {
                "prompt_tokens": "N/A",
                "completion_tokens": "N/A",
                "total_tokens": "N/A"
            },
            "request_duration_seconds": request_duration_seconds
        }

        if process.returncode == 0:
            generated_content = process.stdout.strip()
            if not generated_content:
                generated_content = "Codely AI 执行成功但未返回内容"
        else:
            generated_content = f"Codely AI 执行失败: {process.stderr.strip()}"

        response_json = {
            "stdout": process.stdout,
            "stderr": process.stderr,
            "returncode": process.returncode,
            "command": ' '.join(codely_cmd) if isinstance(codely_cmd, list) else codely_cmd
        }

        return info, generated_content, response_json

    except Exception as e:
        error_info = {"error": str(e)}
        return error_info, f"Codely AI 调用异常: {str(e)}", {"error": str(e)}

    finally:
        if tmp_path:
            try:
                os.unlink(tmp_path)
            except OSError:
                pass


def send_to_github_pr(info, ai_review, owner_repo, pr_number, github_token):
    """发送审查结果到GitHub PR评论"""
    if not pr_number:
        print("没有提供PR编号，跳过发送到GitHub")
        return False

    api_url = f"https://api.github.com/repos/{owner_repo}/issues/{pr_number}/comments"

    headers = {
        "Accept": "application/vnd.github.v3+json",
        "Authorization": f"token {github_token}",
        "X-GitHub-Api-Version": "2022-11-28"
    }

    content = f"""
<details>
<summary>🤖 AI Code Review 详情</summary>

```
Model: {info.get("model_name", "N/A")}
Duration: {info.get("request_duration_seconds", "N/A")}s
```

</details>

{ai_review}
    """

    data = {"body": content.strip()}

    try:
        response = requests.post(api_url, headers=headers, json=data, timeout=30)

        if response.status_code in [200, 201]:
            print(f"成功发送审查结果到GitHub PR #{pr_number}")
            return True
        else:
            print(f"发送到GitHub失败，状态码: {response.status_code}, 响应: {response.text}")
            return False

    except Exception as e:
        print(f"发送到GitHub异常: {str(e)}")
        return False


def send_to_feishu(ai_review, project_name, commit_sha, pr_number=None, pr_url=None,
                   commit_url=None, webhook_url=None):
    """发送审查结果到飞书"""
    if pr_number:
        button_text = "查看 Pull Request"
        button_url = pr_url
    else:
        button_text = "查看完整提交"
        button_url = commit_url

    card_payload = {
        "msg_type": "interactive",
        "card": {
            "schema": "2.0",
            "config": {
                "update_multi": True,
                "style": {
                    "text_size": {
                        "normal_v2": {
                            "default": "normal",
                            "pc": "normal",
                            "mobile": "heading"
                        }
                    }
                }
            },
            "body": {
                "direction": "vertical",
                "padding": "20px 20px 20px 20px",
                "elements": [
                    {
                        "tag": "markdown",
                        "content": f"{ai_review}",
                        "text_align": "left",
                        "text_size": "normal_v2",
                        "margin": "0px 0px 0px 0px"
                    },
                    {
                        "tag": "button",
                        "text": {
                            "tag": "plain_text",
                            "content": button_text
                        },
                        "type": "default",
                        "width": "default",
                        "size": "medium",
                        "behaviors": [
                            {
                                "type": "open_url",
                                "default_url": button_url,
                                "pc_url": "",
                                "ios_url": "",
                                "android_url": ""
                            }
                        ],
                        "margin": "0px 0px 0px 0px"
                    }
                ]
            },
            "header": {
                "title": {
                    "tag": "plain_text",
                    "content": "📋 AI Code Review 报告"
                },
                "subtitle": {
                    "tag": "plain_text",
                    "content": ""
                },
                "template": "blue",
                "padding": "12px 12px 12px 12px"
            }
        }
    }

    text_payload = {
        "msg_type": "text",
        "content": {
            "text": f"AI Code Review for {project_name} ({commit_sha[:8]})\n\n{ai_review}"
        }
    }

    try:
        response = requests.post(
            webhook_url,
            headers={"Content-Type": "application/json"},
            json=card_payload
        )

        if response.status_code == 200 and response.json().get("code") == 0:
            print("富文本通知发送成功")
            return True

        print("富文本消息发送失败，尝试发送普通文本消息...")
        response = requests.post(
            webhook_url,
            headers={"Content-Type": "application/json"},
            json=text_payload
        )

        if response.status_code == 200 and response.json().get("code") == 0:
            print("普通文本通知发送成功")
            return True
        else:
            print(f"所有通知方式均失败，响应: {response.text}")
            return False

    except Exception as e:
        print(f"发送通知异常: {str(e)}")
        return False


def main():
    parser = argparse.ArgumentParser(description="AI 代码审查工具 (GitHub)")
    parser.add_argument("--local-test", action="store_true", help="使用mock数据进行本地测试")
    args = parser.parse_args()

    # GitHub Actions 环境变量
    owner_repo = os.getenv("GITHUB_REPOSITORY", "")
    pr_number = os.getenv("PR_NUMBER", "")
    commit_sha = os.getenv("GITHUB_SHA", "")
    github_token = os.getenv("GITHUB_TOKEN", "")
    codely_token = os.getenv("CODELY_TOKEN", "")
    feishu_webhook_url = os.getenv("FEISHU_WEBHOOK_URL", "")

    # 从 owner_repo 提取项目名
    project_name = owner_repo.split('/')[-1] if owner_repo else "unknown-project"

    if args.local_test:
        print("启用本地测试模式，使用mock数据")
        diff_content = get_mock_diff_data()
        project_name = project_name or "aesir-inspector"
        commit_sha = commit_sha or "abc1234567890"
        pr_number = pr_number or "1"
    else:
        diff_content = get_pr_diff_via_api(
            owner_repo=owner_repo,
            pr_number=pr_number,
            github_token=github_token
        )

    filtered_diff = filter_diff(diff_content)

    if not filtered_diff:
        print("没有需要审查的代码变更，跳过审查流程")
        return 0

    print("使用 Codely AI 做代码审查")

    if not check_codely_installed():
        print("Codely CLI 未安装或不可用。请先安装：curl -fsSL https://codesearch-plugins.tos-cn-shanghai.volces.com/codely-cli/install.sh | bash")
        return 1

    info_codely, ai_review_codely, response_json_codely = call_codely_for_review(filtered_diff, codely_token)

    if "error" in info_codely:
        print(f"代码审查执行失败: {ai_review_codely}")
        with open("ai_review_codely.txt", "w", encoding="utf-8") as f:
            f.write(ai_review_codely)
        with open("response_codely.json", "w", encoding="utf-8") as f:
            json.dump(response_json_codely, f, indent=2, ensure_ascii=False)
        return 1

    with open("ai_review_codely.txt", "w", encoding="utf-8") as f:
        f.write(ai_review_codely)

    with open("response_codely.json", "w", encoding="utf-8") as f:
        json.dump(response_json_codely, f, indent=2, ensure_ascii=False)

    # 发送到 GitHub PR 评论
    if args.local_test:
        print("本地测试模式，跳过发送到GitHub")
        github_success = True
    else:
        github_success = send_to_github_pr(
            info=info_codely,
            ai_review=ai_review_codely,
            owner_repo=owner_repo,
            pr_number=pr_number,
            github_token=github_token
        )

    # 发送到飞书
    feishu_success = False
    if feishu_webhook_url:
        github_base_url = f"https://github.com/{owner_repo}"
        feishu_success = send_to_feishu(
            ai_review=ai_review_codely,
            project_name=project_name,
            commit_sha=commit_sha,
            pr_number=pr_number,
            pr_url=f"{github_base_url}/pull/{pr_number}" if pr_number else None,
            commit_url=f"{github_base_url}/commit/{commit_sha}" if commit_sha else None,
            webhook_url=feishu_webhook_url
        )
    else:
        feishu_success = True
        print("没有配置飞书Webhook URL，跳过发送到飞书")

    return 0 if (feishu_success and github_success) else 1


if __name__ == "__main__":
    sys.exit(main())

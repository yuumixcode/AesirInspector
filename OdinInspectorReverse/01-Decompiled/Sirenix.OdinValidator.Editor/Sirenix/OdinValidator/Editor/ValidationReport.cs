using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sirenix.OdinValidator.Editor
{
	public static class ValidationReport
	{
		[Serializable]
		public class SessionReport
		{
			public string SessionName;

			public int ValidCount;

			public int WarningCount;

			public int ErrorCount;

			public List<ValidationReportItem> Reports = new List<ValidationReportItem>();
		}

		[Serializable]
		public class ValidationReportItem
		{
			public string ValidatorName;

			public string ValidatorMessage;

			public string AssetPath;

			public string HierarchyPath;

			public string PropertyPath;

			public double ValidationTimeMS;

			public ValidationResultType ResultType;
		}

		private const string HtmlReportTemplate = "\r\n<!DOCTYPE html>\r\n<html lang='en'>\r\n<head>\r\n\t<meta charset='UTF-8'>\r\n\t<meta name='viewport' content='width=device-width, initial-scale=1.0'>\r\n\t<meta http-equiv='X-UA-Compatible' content='ie=edge'>\r\n\t<title>{{ReportName}} Report</title>\r\n\t<style>\r\n\t\t:root {\r\n\t\t\t--color-text: #C4C4C4;\r\n\t\t\t--color-border: #222222;\r\n\t\t\t--color-hover: #4C4C4C;\r\n\t\t\t--color-background: #383838;\r\n\t\t\t--color-result-count-hover: #565656;\r\n\t\t\t--color-switch-on: #33C107;\r\n\t\t\t--color-switch-off: #FF534A;\r\n\t\t\t--color-toolbar: #282828;\r\n\t\t}\r\n\r\n\t\thtml {\r\n\t\t\tbox-sizing: border-box;\r\n\t\t\tfont-size: 62.5%;\r\n\t\t}\r\n\r\n\t\tbody {\r\n\t\t\tpadding: 3rem;\r\n\t\t\tfont-size: 1.6rem;\r\n\t\t\tcolor: var(--color-text);\r\n\t\t\tfont-family: 'Arial', sans-serif;\r\n\t\t\tbackground-color: var(--color-background);\r\n\t\t}\r\n\r\n\t\th1 {\r\n\t\t\ttext-align: center;\r\n\t\t}\r\n\r\n\t\t.hidden {\r\n\t\t\tdisplay: none;\r\n\t\t}\r\n\r\n\t\t.inactive::before {\r\n\t\t\tfilter: contrast(0%);\r\n\t\t}\r\n\r\n\t\t.info {\r\n\t\t\tgap: 5rem;\r\n\t\t\tdisplay: flex;\r\n\t\t\tmargin-bottom: 3rem;\r\n\t\t}\r\n\r\n\t\t.info>div {\r\n\t\t\tdisplay: grid;\r\n\t\t}\r\n\r\n\t\t.container {\r\n\t\t\tborder: 0.1rem solid var(--color-border);\r\n\t\t\tborder-bottom-left-radius: 0.5rem;\r\n\t\t\tborder-bottom-right-radius: 0.5rem;\r\n\t\t\tborder-top: none;\r\n\t\t\toverflow-x: scroll;\r\n\t\t\twhite-space: nowrap;\r\n\t\t}\r\n\r\n\t\t.toolbar {\r\n\t\t\tdisplay: flex;\r\n\t\t\tjustify-content: flex-end;\r\n\t\t\theight: 3rem;\r\n\t\t\tborder-top-left-radius: 0.5rem;\r\n\t\t\tborder-top-right-radius: 0.5rem;\r\n\t\t\tborder: 0.1rem solid var(--color-border);\r\n\t\t\tbackground-color: var(--color-toolbar);\r\n\t\t\tborder-bottom: none;\r\n\t\t\toverflow: hidden;\r\n\t\t}\r\n\r\n\t\t.result-count-toggle {\r\n\t\t\tborder: none;\r\n\t\t\theight: 100%;\r\n\t\t\tcolor: var(--color-text);\r\n\t\t\tbackground-color: var(--color-toolbar);\r\n\t\t\tpadding-top: 0.25rem;\r\n\t\t\tcursor: pointer;\r\n\t\t}\r\n\r\n\t\t.result-count-toggle.active,\r\n\t\t.result-count-toggle:hover {\r\n\t\t\tbackground-color: var(--color-hover);\r\n\t\t}\r\n\r\n\t\t.result-count-toggle:first-child {\r\n\t\t\tborder-left: 0.1rem solid var(--color-border);\r\n\t\t\tborder-right: 0.1rem solid var(--color-border);\r\n\t\t}\r\n\r\n\t\t.result-count-toggle:last-child {\r\n\t\t\tborder-top-right-radius: 0.5rem;\r\n\t\t\tborder-left: 0.1rem solid var(--color-border);\r\n\t\t}\r\n\r\n        .valid::before {\r\n            vertical-align: middle;\r\n            padding-right: 0.5rem;\r\n            content: url(\"data:image/svg+xml,%3Csvg fill='none' width='18' height='18' viewBox='0 0 32 32' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath clip-rule='evenodd' d='m16 30c7.732 0 14-6.268 14-14 0-7.73199-6.268-14-14-14-7.73199 0-14 6.26801-14 14 0 7.732 6.26801 14 14 14zm5.8228-18.0446c-.5858-.5858-1.5355-.5858-2.1213 0l-4.8124 4.8124-1.7071-1.7071c-.5858-.5858-1.5355-.5858-2.1213 0s-.5858 1.5355 0 2.1213l3.8318 3.8318 2.1213-2.1213-.0034-.0034 4.8124-4.8124c.5858-.5858.5858-1.5355 0-2.1213z' fill='%2333C107' fill-rule='evenodd'/%3E%3C/svg%3E\");\r\n        }\r\n\r\n        .warning::before {\r\n            vertical-align: middle;\r\n            padding-right: 0.5rem;\r\n            content: url(\"data:image/svg+xml,%3Csvg fill='none' width='18' height='18' viewBox='0 0 32 32' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath clip-rule='evenodd' d='m16.866 3.50002c-.3849-.66669-1.3471-.66669-1.732 0l-13.85642 23.99998c-.384948.6667.09619 1.5.86603 1.5h27.71279c.7698 0 1.251-.8333.866-1.5zm-.866 6.49998c-1.074 0-1.9161.9222-1.8189 1.9918l.7284 8.0123c.0512.564.5241.9959 1.0905.9959.5663 0 1.0392-.4319 1.0905-.9959l.7284-8.0123c.0972-1.0696-.7449-1.9918-1.8189-1.9918zm0 16c1.1045 0 2-.8954 2-2 0-1.1045-.8955-2-2-2-1.1046 0-2 .8955-2 2 0 1.1046.8954 2 2 2z' fill='%23F6C344' fill-rule='evenodd'/%3E%3C/svg%3E\");\r\n        }\r\n\r\n        .error::before {\r\n            vertical-align: middle;\r\n            padding-right: 0.5rem;\r\n            content: url(\"data:image/svg+xml,%3Csvg fill='none' width='18' height='18' viewBox='0 0 32 32' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath clip-rule='evenodd' d='m29.5204 10.3748-7.9584-7.8628-11.1873.06761-7.86271 7.95839.06762 11.1872 7.95839 7.8628 11.1872-.0676 7.8628-7.9584zm-13.5204-2.3748c-1.074 0-1.9162.9222-1.8189 1.99179l.7284 8.01231c.0512.564.5241.9959 1.0905.9959s1.0393-.4319 1.0905-.9959l.7284-8.01231c.0973-1.06959-.7449-1.99179-1.8189-1.99179zm0 16c1.1046 0 2-.8954 2-2s-.8954-2-2-2-2 .8954-2 2 .8954 2 2 2z' fill='%23FF534A' fill-rule='evenodd'/%3E%3C/svg%3E\");\r\n        }\r\n\t\t\r\n\t\ttable {\r\n\t\t\twidth: 100%;\r\n\t\t\tborder-collapse: collapse;\r\n\t\t}\r\n\r\n\t\ttable td,\r\n\t\ttable th {\r\n\t\t\ttext-align: left;\r\n\t\t\tborder: 0.1rem solid var(--color-border);\r\n\t\t}\r\n\r\n\t\tth,\r\n\t\ttd {\r\n\t\t\tpadding: 0.5em;\r\n\t\t}\r\n\r\n\t\tth:first-child,\r\n\t\ttd:first-child {\r\n\t\t\tborder-left: none;\r\n\t\t}\r\n\r\n\t\tth:last-child,\r\n\t\ttd:last-child {\r\n\t\t\tborder-right: none;\r\n\t\t}\r\n\r\n\t\t::-webkit-scrollbar {\r\n\t\t\tborder-bottom-left-radius: 0.4rem;\r\n\t\t\tborder-bottom-right-radius: 0.4rem;\r\n\t\t\tbackground-color: var(--color-toolbar);\r\n\t\t}\r\n\r\n\t\t::-webkit-scrollbar-thumb {\r\n\t\t\tborder-radius: 999rem;\r\n\t\t\tbackground: var(--color-background);\r\n\t\t\tborder: 0.5rem solid var(--color-toolbar)\r\n\t\t}\r\n\r\n\t\t::-webkit-scrollbar-thumb:hover {\r\n\t\t\tbackground: var(--color-hover);\r\n\t\t}\r\n\t</style>\r\n</head>\r\n<body>\r\n\t<h1>{{ReportName}}</h1>\r\n\t<div class='info'>\r\n\t\t<div>\r\n\t\t\t<span><b>Product Name:</b> {{ProductName}}</span>\r\n\t\t\t<span><b>Operating System:</b> {{OperatingSystem}}</span>\r\n\t\t\t<span><b>Unity Version:</b> {{UnityVersion}}</span>\r\n\t\t\t<span><b>Odin Version:</b> {{OdinVersion}}</span>\r\n\t\t\t<span><b>Render Pipeline:</b> {{RenderPipeline}}</span>\r\n\t\t</div>\r\n\t</div>\r\n\t<div class='info'>\r\n\t\t<div>\r\n\t\t\t<label><input col='message' type='checkbox' checked><span>Message</span></label>\r\n\t\t\t<label><input col='validator' type='checkbox' checked><span>Validator</span></label>\r\n\t\t\t<label><input col='asset-path' type='checkbox' checked><span>Asset Path</span></label>\r\n\t\t\t<label><input col='hierarchy-path' type='checkbox' checked><span>Hierarchy Path</span></label>\r\n\t\t\t<label><input col='property-path' type='checkbox' checked><span>Property Path</span></label>\r\n\t\t\t<label><input col='ms' type='checkbox' checked><span>Milliseconds</span></label>\r\n\t\t</div>\r\n\t</div>\r\n\t<div class='toolbar'>\r\n\t\t<button class='result-count-toggle valid'>{{ValidCount}}</button>\r\n\t\t<button class='result-count-toggle warning'>{{WarningCount}}</button>\r\n\t\t<button class='result-count-toggle error'>{{ErrorCount}}</button>\r\n\t</div>\r\n\t<div class='container'>\r\n\t\t<table>\r\n\t\t\t<tr>\r\n\t\t\t\t<th class='message'>Message</th>\r\n\t\t\t\t<th class='validator'>Validator</th>\r\n\t\t\t\t<th class='asset-path'>Asset Path</th>\r\n\t\t\t\t<th class='hierarchy-path'>Hierarchy Path</th>\r\n\t\t\t\t<th class='property-path'>Property Path</th>\r\n\t\t\t\t<th class='ms'>Milliseconds</th>\r\n\t\t\t</tr>\r\n\t\t\t{{Rows}}\r\n\t\t</table>\r\n\t</div>\r\n\t<script>\r\n\t\tlet checkboxes = document.querySelectorAll(\"input[type = 'checkbox']\");\r\n\t\tfor (let i = 0; i < checkboxes.length; i++) {\r\n\t\t\tlet checkbox = checkboxes[i];\r\n\t\t\tcheckbox.addEventListener('change', () => {\r\n\t\t\t\tvar columns = document.getElementsByClassName(`${checkbox.getAttribute('col')}`);\r\n\t\t\t\tfor (let column of columns) {\r\n\t\t\t\t\tcolumn.classList.toggle('hidden');\r\n\t\t\t\t}\r\n\t\t\t});\r\n\t\t}\r\n\r\n\t\tsetupResultTypeToggle(document.getElementsByClassName('result-count-toggle valid'), 'valid');\r\n\t\tsetupResultTypeToggle(document.getElementsByClassName('result-count-toggle warning'), 'warning');\r\n\t\tsetupResultTypeToggle(document.getElementsByClassName('result-count-toggle error'), 'error');\r\n\r\n\t\tfunction setupResultTypeToggle(buttons, name) {\r\n\t\t\tfor (let btn of buttons) {\r\n\t\t\t\tbtn.addEventListener('click', (e) => {\r\n\t\t\t\t\tbtn.classList.toggle('inactive');\r\n\t\t\t\t\tlet entries = btn.parentElement.parentElement.querySelectorAll(`.${name}.entry`);\r\n\t\t\t\t\tfor (let entry of entries) {\r\n\t\t\t\t\t\tentry.parentElement.classList.toggle('hidden');\r\n\t\t\t\t\t}\r\n\t\t\t\t});\r\n\t\t\t}\r\n\t\t}\r\n\t</script>\r\n</body>\r\n</html>";

		private const string HtmlReportTableEntryTemplate = "\r\n<tr>\r\n    <td class='{{ResultType}} entry message'>{{ValidatorMessage}}</td>\r\n    <td class='validator'>{{ValidatorName}}</td>\r\n    <td class='asset-path'>{{ValidatorAssetPath}}</td>\r\n    <td class='hierarchy-path'>{{ValidatorHierarchyPath}}</td>\r\n    <td class='property-path'>{{ValidatorPropertyPath}}</td>\r\n    <td class='ms'>{{ValidatorMs}}</td>\r\n</tr>";

		public static SessionReport CreateSessionReport(string name, IList<PersistentValidationResult> results)
		{
			SessionReport report = new SessionReport
			{
				SessionName = name
			};
			foreach (PersistentValidationResult result in results)
			{
				if (result.ResultType != ValidationResultType.IgnoreResult)
				{
					switch (result.ResultType)
					{
					case ValidationResultType.Valid:
						report.ValidCount++;
						break;
					case ValidationResultType.Error:
						report.ErrorCount++;
						break;
					case ValidationResultType.Warning:
						report.WarningCount++;
						break;
					}
					report.Reports.Add(new ValidationReportItem
					{
						ValidatorName = result.ValidatorType.GetNiceValidatorTypeName(),
						ValidatorMessage = result.Message,
						AssetPath = result.DynamicObjectAddress.LatestAddress.AssetPath,
						HierarchyPath = result.DynamicObjectAddress.LatestAddress.Hierarchy.ToString(),
						PropertyPath = result.Path,
						ValidationTimeMS = result.Batch.ValidationTimeMS,
						ResultType = result.ResultType
					});
				}
			}
			return report;
		}

		public static string ToJson(this ValidationSession session, bool prettyPrint = false)
		{
			PersistentValidationResult[] validationResults = session.GetCurrentResults();
			SessionReport sessionReport = CreateSessionReport(session.Name, validationResults);
			return JsonUtility.ToJson(sessionReport, prettyPrint);
		}

		public static string ToHtml(this ValidationSession session)
		{
			PersistentValidationResult[] validationResults = session.GetCurrentResults();
			SessionReport sessionReport = CreateSessionReport(session.Name, validationResults);
			StringBuilder html = new StringBuilder("\r\n<!DOCTYPE html>\r\n<html lang='en'>\r\n<head>\r\n\t<meta charset='UTF-8'>\r\n\t<meta name='viewport' content='width=device-width, initial-scale=1.0'>\r\n\t<meta http-equiv='X-UA-Compatible' content='ie=edge'>\r\n\t<title>{{ReportName}} Report</title>\r\n\t<style>\r\n\t\t:root {\r\n\t\t\t--color-text: #C4C4C4;\r\n\t\t\t--color-border: #222222;\r\n\t\t\t--color-hover: #4C4C4C;\r\n\t\t\t--color-background: #383838;\r\n\t\t\t--color-result-count-hover: #565656;\r\n\t\t\t--color-switch-on: #33C107;\r\n\t\t\t--color-switch-off: #FF534A;\r\n\t\t\t--color-toolbar: #282828;\r\n\t\t}\r\n\r\n\t\thtml {\r\n\t\t\tbox-sizing: border-box;\r\n\t\t\tfont-size: 62.5%;\r\n\t\t}\r\n\r\n\t\tbody {\r\n\t\t\tpadding: 3rem;\r\n\t\t\tfont-size: 1.6rem;\r\n\t\t\tcolor: var(--color-text);\r\n\t\t\tfont-family: 'Arial', sans-serif;\r\n\t\t\tbackground-color: var(--color-background);\r\n\t\t}\r\n\r\n\t\th1 {\r\n\t\t\ttext-align: center;\r\n\t\t}\r\n\r\n\t\t.hidden {\r\n\t\t\tdisplay: none;\r\n\t\t}\r\n\r\n\t\t.inactive::before {\r\n\t\t\tfilter: contrast(0%);\r\n\t\t}\r\n\r\n\t\t.info {\r\n\t\t\tgap: 5rem;\r\n\t\t\tdisplay: flex;\r\n\t\t\tmargin-bottom: 3rem;\r\n\t\t}\r\n\r\n\t\t.info>div {\r\n\t\t\tdisplay: grid;\r\n\t\t}\r\n\r\n\t\t.container {\r\n\t\t\tborder: 0.1rem solid var(--color-border);\r\n\t\t\tborder-bottom-left-radius: 0.5rem;\r\n\t\t\tborder-bottom-right-radius: 0.5rem;\r\n\t\t\tborder-top: none;\r\n\t\t\toverflow-x: scroll;\r\n\t\t\twhite-space: nowrap;\r\n\t\t}\r\n\r\n\t\t.toolbar {\r\n\t\t\tdisplay: flex;\r\n\t\t\tjustify-content: flex-end;\r\n\t\t\theight: 3rem;\r\n\t\t\tborder-top-left-radius: 0.5rem;\r\n\t\t\tborder-top-right-radius: 0.5rem;\r\n\t\t\tborder: 0.1rem solid var(--color-border);\r\n\t\t\tbackground-color: var(--color-toolbar);\r\n\t\t\tborder-bottom: none;\r\n\t\t\toverflow: hidden;\r\n\t\t}\r\n\r\n\t\t.result-count-toggle {\r\n\t\t\tborder: none;\r\n\t\t\theight: 100%;\r\n\t\t\tcolor: var(--color-text);\r\n\t\t\tbackground-color: var(--color-toolbar);\r\n\t\t\tpadding-top: 0.25rem;\r\n\t\t\tcursor: pointer;\r\n\t\t}\r\n\r\n\t\t.result-count-toggle.active,\r\n\t\t.result-count-toggle:hover {\r\n\t\t\tbackground-color: var(--color-hover);\r\n\t\t}\r\n\r\n\t\t.result-count-toggle:first-child {\r\n\t\t\tborder-left: 0.1rem solid var(--color-border);\r\n\t\t\tborder-right: 0.1rem solid var(--color-border);\r\n\t\t}\r\n\r\n\t\t.result-count-toggle:last-child {\r\n\t\t\tborder-top-right-radius: 0.5rem;\r\n\t\t\tborder-left: 0.1rem solid var(--color-border);\r\n\t\t}\r\n\r\n        .valid::before {\r\n            vertical-align: middle;\r\n            padding-right: 0.5rem;\r\n            content: url(\"data:image/svg+xml,%3Csvg fill='none' width='18' height='18' viewBox='0 0 32 32' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath clip-rule='evenodd' d='m16 30c7.732 0 14-6.268 14-14 0-7.73199-6.268-14-14-14-7.73199 0-14 6.26801-14 14 0 7.732 6.26801 14 14 14zm5.8228-18.0446c-.5858-.5858-1.5355-.5858-2.1213 0l-4.8124 4.8124-1.7071-1.7071c-.5858-.5858-1.5355-.5858-2.1213 0s-.5858 1.5355 0 2.1213l3.8318 3.8318 2.1213-2.1213-.0034-.0034 4.8124-4.8124c.5858-.5858.5858-1.5355 0-2.1213z' fill='%2333C107' fill-rule='evenodd'/%3E%3C/svg%3E\");\r\n        }\r\n\r\n        .warning::before {\r\n            vertical-align: middle;\r\n            padding-right: 0.5rem;\r\n            content: url(\"data:image/svg+xml,%3Csvg fill='none' width='18' height='18' viewBox='0 0 32 32' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath clip-rule='evenodd' d='m16.866 3.50002c-.3849-.66669-1.3471-.66669-1.732 0l-13.85642 23.99998c-.384948.6667.09619 1.5.86603 1.5h27.71279c.7698 0 1.251-.8333.866-1.5zm-.866 6.49998c-1.074 0-1.9161.9222-1.8189 1.9918l.7284 8.0123c.0512.564.5241.9959 1.0905.9959.5663 0 1.0392-.4319 1.0905-.9959l.7284-8.0123c.0972-1.0696-.7449-1.9918-1.8189-1.9918zm0 16c1.1045 0 2-.8954 2-2 0-1.1045-.8955-2-2-2-1.1046 0-2 .8955-2 2 0 1.1046.8954 2 2 2z' fill='%23F6C344' fill-rule='evenodd'/%3E%3C/svg%3E\");\r\n        }\r\n\r\n        .error::before {\r\n            vertical-align: middle;\r\n            padding-right: 0.5rem;\r\n            content: url(\"data:image/svg+xml,%3Csvg fill='none' width='18' height='18' viewBox='0 0 32 32' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath clip-rule='evenodd' d='m29.5204 10.3748-7.9584-7.8628-11.1873.06761-7.86271 7.95839.06762 11.1872 7.95839 7.8628 11.1872-.0676 7.8628-7.9584zm-13.5204-2.3748c-1.074 0-1.9162.9222-1.8189 1.99179l.7284 8.01231c.0512.564.5241.9959 1.0905.9959s1.0393-.4319 1.0905-.9959l.7284-8.01231c.0973-1.06959-.7449-1.99179-1.8189-1.99179zm0 16c1.1046 0 2-.8954 2-2s-.8954-2-2-2-2 .8954-2 2 .8954 2 2 2z' fill='%23FF534A' fill-rule='evenodd'/%3E%3C/svg%3E\");\r\n        }\r\n\t\t\r\n\t\ttable {\r\n\t\t\twidth: 100%;\r\n\t\t\tborder-collapse: collapse;\r\n\t\t}\r\n\r\n\t\ttable td,\r\n\t\ttable th {\r\n\t\t\ttext-align: left;\r\n\t\t\tborder: 0.1rem solid var(--color-border);\r\n\t\t}\r\n\r\n\t\tth,\r\n\t\ttd {\r\n\t\t\tpadding: 0.5em;\r\n\t\t}\r\n\r\n\t\tth:first-child,\r\n\t\ttd:first-child {\r\n\t\t\tborder-left: none;\r\n\t\t}\r\n\r\n\t\tth:last-child,\r\n\t\ttd:last-child {\r\n\t\t\tborder-right: none;\r\n\t\t}\r\n\r\n\t\t::-webkit-scrollbar {\r\n\t\t\tborder-bottom-left-radius: 0.4rem;\r\n\t\t\tborder-bottom-right-radius: 0.4rem;\r\n\t\t\tbackground-color: var(--color-toolbar);\r\n\t\t}\r\n\r\n\t\t::-webkit-scrollbar-thumb {\r\n\t\t\tborder-radius: 999rem;\r\n\t\t\tbackground: var(--color-background);\r\n\t\t\tborder: 0.5rem solid var(--color-toolbar)\r\n\t\t}\r\n\r\n\t\t::-webkit-scrollbar-thumb:hover {\r\n\t\t\tbackground: var(--color-hover);\r\n\t\t}\r\n\t</style>\r\n</head>\r\n<body>\r\n\t<h1>{{ReportName}}</h1>\r\n\t<div class='info'>\r\n\t\t<div>\r\n\t\t\t<span><b>Product Name:</b> {{ProductName}}</span>\r\n\t\t\t<span><b>Operating System:</b> {{OperatingSystem}}</span>\r\n\t\t\t<span><b>Unity Version:</b> {{UnityVersion}}</span>\r\n\t\t\t<span><b>Odin Version:</b> {{OdinVersion}}</span>\r\n\t\t\t<span><b>Render Pipeline:</b> {{RenderPipeline}}</span>\r\n\t\t</div>\r\n\t</div>\r\n\t<div class='info'>\r\n\t\t<div>\r\n\t\t\t<label><input col='message' type='checkbox' checked><span>Message</span></label>\r\n\t\t\t<label><input col='validator' type='checkbox' checked><span>Validator</span></label>\r\n\t\t\t<label><input col='asset-path' type='checkbox' checked><span>Asset Path</span></label>\r\n\t\t\t<label><input col='hierarchy-path' type='checkbox' checked><span>Hierarchy Path</span></label>\r\n\t\t\t<label><input col='property-path' type='checkbox' checked><span>Property Path</span></label>\r\n\t\t\t<label><input col='ms' type='checkbox' checked><span>Milliseconds</span></label>\r\n\t\t</div>\r\n\t</div>\r\n\t<div class='toolbar'>\r\n\t\t<button class='result-count-toggle valid'>{{ValidCount}}</button>\r\n\t\t<button class='result-count-toggle warning'>{{WarningCount}}</button>\r\n\t\t<button class='result-count-toggle error'>{{ErrorCount}}</button>\r\n\t</div>\r\n\t<div class='container'>\r\n\t\t<table>\r\n\t\t\t<tr>\r\n\t\t\t\t<th class='message'>Message</th>\r\n\t\t\t\t<th class='validator'>Validator</th>\r\n\t\t\t\t<th class='asset-path'>Asset Path</th>\r\n\t\t\t\t<th class='hierarchy-path'>Hierarchy Path</th>\r\n\t\t\t\t<th class='property-path'>Property Path</th>\r\n\t\t\t\t<th class='ms'>Milliseconds</th>\r\n\t\t\t</tr>\r\n\t\t\t{{Rows}}\r\n\t\t</table>\r\n\t</div>\r\n\t<script>\r\n\t\tlet checkboxes = document.querySelectorAll(\"input[type = 'checkbox']\");\r\n\t\tfor (let i = 0; i < checkboxes.length; i++) {\r\n\t\t\tlet checkbox = checkboxes[i];\r\n\t\t\tcheckbox.addEventListener('change', () => {\r\n\t\t\t\tvar columns = document.getElementsByClassName(`${checkbox.getAttribute('col')}`);\r\n\t\t\t\tfor (let column of columns) {\r\n\t\t\t\t\tcolumn.classList.toggle('hidden');\r\n\t\t\t\t}\r\n\t\t\t});\r\n\t\t}\r\n\r\n\t\tsetupResultTypeToggle(document.getElementsByClassName('result-count-toggle valid'), 'valid');\r\n\t\tsetupResultTypeToggle(document.getElementsByClassName('result-count-toggle warning'), 'warning');\r\n\t\tsetupResultTypeToggle(document.getElementsByClassName('result-count-toggle error'), 'error');\r\n\r\n\t\tfunction setupResultTypeToggle(buttons, name) {\r\n\t\t\tfor (let btn of buttons) {\r\n\t\t\t\tbtn.addEventListener('click', (e) => {\r\n\t\t\t\t\tbtn.classList.toggle('inactive');\r\n\t\t\t\t\tlet entries = btn.parentElement.parentElement.querySelectorAll(`.${name}.entry`);\r\n\t\t\t\t\tfor (let entry of entries) {\r\n\t\t\t\t\t\tentry.parentElement.classList.toggle('hidden');\r\n\t\t\t\t\t}\r\n\t\t\t\t});\r\n\t\t\t}\r\n\t\t}\r\n\t</script>\r\n</body>\r\n</html>");
			html.Replace("{{ReportName}}", sessionReport.SessionName);
			html.Replace("{{ProductName}}", Application.productName);
			html.Replace("{{OperatingSystem}}", SystemInfo.operatingSystem);
			html.Replace("{{UnityVersion}}", Application.unityVersion);
			html.Replace("{{OdinVersion}}", OdinInspectorVersion.Version);
			html.Replace("{{RenderPipeline}}", GraphicsSettings.currentRenderPipeline?.name ?? "Builtin");
			html.Replace("{{ValidCount}}", sessionReport.ValidCount.ToString());
			html.Replace("{{WarningCount}}", sessionReport.WarningCount.ToString());
			html.Replace("{{ErrorCount}}", sessionReport.ErrorCount.ToString());
			StringBuilder rows = new StringBuilder();
			foreach (ValidationReportItem report in sessionReport.Reports.OrderBy((ValidationReportItem validationReportItem) => validationReportItem.ResultType))
			{
				StringBuilder row = new StringBuilder("\r\n<tr>\r\n    <td class='{{ResultType}} entry message'>{{ValidatorMessage}}</td>\r\n    <td class='validator'>{{ValidatorName}}</td>\r\n    <td class='asset-path'>{{ValidatorAssetPath}}</td>\r\n    <td class='hierarchy-path'>{{ValidatorHierarchyPath}}</td>\r\n    <td class='property-path'>{{ValidatorPropertyPath}}</td>\r\n    <td class='ms'>{{ValidatorMs}}</td>\r\n</tr>");
				row.Replace("{{ResultType}}", report.ResultType.ToString().ToLower());
				row.Replace("{{ValidatorMessage}}", report.ValidatorMessage);
				row.Replace("{{ValidatorName}}", report.ValidatorName);
				row.Replace("{{ValidatorAssetPath}}", report.AssetPath);
				row.Replace("{{ValidatorHierarchyPath}}", report.HierarchyPath);
				row.Replace("{{ValidatorPropertyPath}}", report.PropertyPath);
				row.Replace("{{ValidatorMs}}", report.ValidationTimeMS.ToString());
				rows.Append((object?)row);
			}
			html.Replace("{{Rows}}", rows.ToString());
			return html.ToString();
		}
	}
}

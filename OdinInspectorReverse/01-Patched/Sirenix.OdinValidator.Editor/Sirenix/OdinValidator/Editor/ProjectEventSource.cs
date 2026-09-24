namespace Sirenix.OdinValidator.Editor
{
	public enum ProjectEventSource
	{
		Unknown,
		TransformModification,
		SceneMonitor,
		MonitorAssetProcessorChanges,
		UndoOrRedo,
		UndoOrRedoModification,
		OdinValidationEvents,
		RevalidationDuringFixes,
		Other
	}
}

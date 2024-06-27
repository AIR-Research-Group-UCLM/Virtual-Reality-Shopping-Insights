using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HeatmapVisualization
{
	public class HeatmapGenerationInEditor : MonoBehaviour
	{
		#region Settings
		[SerializeField] private string directoryAndFileNameWithExtension;

		[SerializeField] private HeadAndHandsTrackingManager hhmanager;
		#endregion


		#region Globals
		private Heatmap ownHeatmap;
		private Heatmap OwnHeatmap { get { if (ownHeatmap == null) { ownHeatmap = GetComponent<Heatmap>(); } return ownHeatmap; } }
		#endregion


		#region Functions
		public void GenerateExampleHeatmap()
		{
			List<Vector3> recordedSessionPoints = Vector3SerializationHelper.DeserializeVector3List(directoryAndFileNameWithExtension);
			OwnHeatmap.GenerateHeatmap(recordedSessionPoints);
		}
		
		#endregion
	}
}

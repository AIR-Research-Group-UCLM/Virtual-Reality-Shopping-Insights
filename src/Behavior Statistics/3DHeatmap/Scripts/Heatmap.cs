using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeatmapSettings = HeatmapVisualization.HeatmapTextureGenerator.Settings;


namespace HeatmapVisualization
{
	public class Heatmap : MonoBehaviour
	{
        #region Settings
        [SerializeField]
        private ComputeShader gaussianComputeShader;
        [SerializeField]
        private Vector3Int resolution = new Vector3Int(64, 64, 64);
        [SerializeField, Range(0.0f, 1.0f)]
        private float cutoffPercentage = 1.0f;
        [SerializeField]
        private float gaussStandardDeviation = 1.0f;
        [SerializeField]
        private Gradient colormap;
        [SerializeField]
        private bool renderOnTop = false;
        [SerializeField]
        private FilterMode textureFilterMode = FilterMode.Bilinear;

        private const int colormapTextureResolution = 256;
        #endregion

        #region Globals
        private MeshRenderer ownRenderer;
        private MeshRenderer OwnRenderer => ownRenderer ?? (ownRenderer = GetComponent<MeshRenderer>());
        private Material OwnRenderersMaterial => !materialIsInstanced ? InstantiateMaterialAndReturn() : OwnRenderer.sharedMaterial;
        private bool materialIsInstanced = false;

        public Bounds BoundsFromTransform => new Bounds(transform.position, transform.localScale);
        #endregion

        #region Functions
        /// <summary>
        /// Generates the heatmap based on the given points.
        /// </summary>
        /// <param name="points">List of points to derive density from.</param>
        public void GenerateHeatmap(List<Vector3> points) => GenerateHeatmap(points.ToArray());

        /// <summary>
        /// Generates the heatmap based on the given points.
        /// </summary>
        /// <param name="points">Array of points to derive density from.</param>
        public void GenerateHeatmap(Vector3[] points)
        {
            ValidatePoints(points);

            var settings = new HeatmapSettings(BoundsFromTransform, resolution, gaussStandardDeviation);
            var heatmapTextureGenerator = new HeatmapTextureGenerator(gaussianComputeShader);
            var heatValues = heatmapTextureGenerator.CalculateHeatTexture(points, settings);

            SetAllMaterialValues(heatValues);
        }

        /// <summary>
        /// Generates the heatmap based on the given heat values.
        /// </summary>
        /// <param name="heatValues">List of heat values.</param>
        public void GenerateHeatmapFromHeatValues(List<float> heatValues) => GenerateHeatmapFromHeatValues(heatValues.ToArray());

        /// <summary>
        /// Generates the heatmap based on the given heat values.
        /// </summary>
        /// <param name="heatValues">Array of heat values.</param>
        public void GenerateHeatmapFromHeatValues(float[] heatValues)
        {
            ValidateHeatValues(heatValues);
            SetAllMaterialValues(heatValues);
        }

        private void SetAllMaterialValues(float[] heatValues)
        {
            float maxHeatFromTexture = GetMaxValue(heatValues);

            SetHeatTexture(heatValues);
            SetMaxHeat(maxHeatFromTexture);
            SetColormap();
            SetCutoffPercentage();
            SetRenderOnTop();
            SetTextureFilterMode();
        }

        private void SetHeatTexture(float[] heatValues)
        {
            var heatTexture = CreateHeatTexture(heatValues);
            OwnRenderersMaterial.SetTexture("_DataTex", heatTexture);
        }

        private Texture3D CreateHeatTexture(float[] heatValues)
        {
            var heatTexture = new Texture3D(resolution.x, resolution.y, resolution.z, TextureFormat.RFloat, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = textureFilterMode
            };
            heatTexture.SetPixelData(heatValues, 0);
            heatTexture.Apply();
            return heatTexture;
        }

        public void SetMaxHeat(float maxHeatFromTexture) => OwnRenderersMaterial.SetFloat("_MaxHeat", maxHeatFromTexture);

        public void SetColormap(Gradient colormap)
        {
            this.colormap = colormap;
            SetColormap();
        }

        public void SetColormap() => OwnRenderersMaterial.SetTexture("_GradientTex", GradientToTexture(colormap, colormapTextureResolution));

        public void SetCutoffPercentage() => OwnRenderersMaterial.SetFloat("_CutoffPercentage", cutoffPercentage);

        public void SetRenderOnTop(bool renderOnTop)
        {
            this.renderOnTop = renderOnTop;
            SetRenderOnTop();
        }

        public void SetRenderOnTop()
        {
            if (renderOnTop)
                OwnRenderersMaterial.DisableKeyword("USE_SCENE_DEPTH");
            else
                OwnRenderersMaterial.EnableKeyword("USE_SCENE_DEPTH");
        }

        public void SetTextureFilterMode(FilterMode textureFilterMode)
        {
            this.textureFilterMode = textureFilterMode;
            SetTextureFilterMode();
        }

        public void SetTextureFilterMode() => OwnRenderersMaterial.GetTexture("_DataTex").filterMode = textureFilterMode;

        /// <summary>
        /// Instantiate the renderer's material to prevent editing the material asset or other heatmaps material.
        /// </summary>
        private Material InstantiateMaterialAndReturn()
        {
            ownRenderer = OwnRenderer;  // Ensure ownRenderer is initialized
            OwnRenderer.sharedMaterial = new Material(OwnRenderer.sharedMaterial);
            materialIsInstanced = true;
            return OwnRenderer.sharedMaterial;
        }

        private static float GetMaxValue(float[] heats) => heats.Length == 0 ? 0 : Mathf.Max(heats);

        /// <summary>
        /// Convert a gradient to a texture so it can be used as a material parameter.
        /// </summary>
        /// <param name="gradient">The gradient to convert.</param>
        /// <param name="resolution">The width of the resulting texture. Height is always 1.</param>
        /// <returns>A texture sampled from the gradient.</returns>
        private static Texture2D GradientToTexture(Gradient gradient, int resolution)
        {
            var texture = new Texture2D(resolution, 1) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            for (int i = 0; i < resolution; i++)
            {
                texture.SetPixel(i, 0, gradient.Evaluate((float)i / (resolution - 1)));
            }
            texture.Apply();
            return texture;
        }

        private void ValidatePoints(Vector3[] points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));

            if (points.Length == 0)
                Debug.LogWarning("Points list for Heatmap generation is empty.", this);
        }

        private void ValidateHeatValues(float[] heatValues)
        {
            if (heatValues == null)
                throw new ArgumentNullException(nameof(heatValues));

            if (heatValues.Length != resolution.x * resolution.y * resolution.z)
                throw new ArgumentException("The length of heatValues does not match resolution setting (x * y * z).");
        }
        #endregion

	}
}
using MapsExt.Utils;
using UnityEngine;

namespace MapsExt
{
	public sealed class LightHandler : MonoBehaviour
	{
		private const float DefaultParticleBoundsX = 80f;
		private const float DefaultParticleBoundsY = 60f;
		private const float DefaultParticleBoundsArea = DefaultParticleBoundsX * DefaultParticleBoundsY;
		private const int DefaultMaxParticles = 1000;
		private const int DefaultParticleEmissionRate = 50;
		private const float DefaultLightAreaX = 200f;
		private const float DefaultLightAreaY = 200f;
		private const float DefaultMapSizeX = 71.12f;
		private const float DefaultMapSizeY = 40f;

		private ParticleSystem[] _particleSystems;
		private SFLight[] _lights;

		private void Start()
		{
			this._particleSystems = this.GetComponentsInChildren<ParticleSystem>(true);
			this._lights = this.GetComponentsInChildren<SFLight>(true);
		}

		private void Update()
		{
			var mapSizeWorld = this.GetCurrentMapWorldSize();

			float scaleRatioX = mapSizeWorld.x / DefaultMapSizeX;
			float scaleRatioY = mapSizeWorld.y / DefaultMapSizeY;

			float newParticleBoundsX = Mathf.Max(DefaultParticleBoundsX, DefaultParticleBoundsX * scaleRatioX);
			float newParticleBoundsY = Mathf.Max(DefaultParticleBoundsY, DefaultParticleBoundsY * scaleRatioY);
			float newParticleBoundsArea = newParticleBoundsX * newParticleBoundsY;

			float uniformScaleRatio = newParticleBoundsArea / DefaultParticleBoundsArea;
			float newMaxParticles = DefaultMaxParticles * uniformScaleRatio;
			float newEmission = DefaultParticleEmissionRate * uniformScaleRatio;

			foreach (var particleSystem in this._particleSystems)
			{
				var shape = particleSystem.shape;
				shape.scale = new Vector3(newParticleBoundsX, newParticleBoundsY, shape.scale.z);

				var mainModule = particleSystem.main;
				var emissionModule = particleSystem.emission;
				mainModule.maxParticles = (int) Mathf.Max(DefaultMaxParticles, newMaxParticles);
				emissionModule.rateOverTimeMultiplier = (int) Mathf.Max(DefaultParticleEmissionRate, newEmission);
			}

			float newLightAreaX = Mathf.Max(DefaultLightAreaX, DefaultLightAreaX * scaleRatioX);
			float newLightAreaY = Mathf.Max(DefaultLightAreaY, DefaultLightAreaY * scaleRatioY);

			foreach (var light in this._lights)
			{
				var lightRect = light.GetComponent<RectTransform>();
				var targetArea = new Vector2(newLightAreaX, newLightAreaY);
				lightRect.sizeDelta = Vector2.Lerp(lightRect.sizeDelta, targetArea, Time.deltaTime * 5f);
			}
		}

		private Vector2 GetCurrentMapWorldSize()
		{
			var map = MapManager.instance.GetCurrentCustomMap();
			if (map == null)
			{
				return new Vector2(DefaultMapSizeX, DefaultMapSizeY);
			}

			return ConversionUtils.ScreenToWorldUnits(map.Settings.MapSize);
		}
	}
}

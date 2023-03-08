using UnityEngine;

namespace MapsExt.UI
{
	public class MapBorder : MonoBehaviour
	{
		protected virtual void Start()
		{
			const float x = 35.56f + 0.05f;
			const float y = 20f + 0.05f;

			var positions = new Vector3[]
				{
					new Vector3(-x, -y),
					new Vector3(x, -y),
					new Vector3(x, y),
					new Vector3(-x, y)
				};

			LineRenderer lr = gameObject.AddComponent<LineRenderer>();
			lr.material = new Material(Shader.Find("Sprites/Default"));
			lr.widthMultiplier = 0.2f;
			lr.positionCount = positions.Length;
			lr.SetPositions(positions);
			lr.startColor = new Color(0.5f, 0.5f, 0.6f, 0.02f);
			lr.endColor = lr.startColor;
			lr.loop = true;
		}
	}
}

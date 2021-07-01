using UnityEngine;

namespace MapEditor.Transformers
{
    public class SmasherTransformer : MonoBehaviour
    {
        public void Start()
        {
            var smasherBase = this.transform.Find("Base");
            var smasherAnim = this.transform.Find("Anim");

            smasherBase.localPosition = new Vector3(0.5f, -0.37f, 0);
            smasherBase.localScale = new Vector3(1.075f, 1, 1);

            smasherAnim.localPosition = new Vector3(0.5f, -0.5f, 0);
        }
    }
}

using Periodiq.Data;
using TMPro;
using UnityEngine;

namespace Periodiq.Table
{
    /// <summary>
    /// One tile on the 3D periodic table. Prefab layout:
    ///   ElementTile (this script + BoxCollider)
    ///   ├─ Body      (Cube mesh, MeshRenderer)
    ///   ├─ Symbol    (TextMeshPro, large, centred)
    ///   ├─ Number    (TextMeshPro, small, top-left)
    ///   └─ Name      (TextMeshPro, tiny, bottom)
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ElementTile : MonoBehaviour
    {
        [SerializeField] MeshRenderer body;
        [SerializeField] TMP_Text symbolText;
        [SerializeField] TMP_Text numberText;
        [SerializeField] TMP_Text nameText;

        public ElementData Data { get; private set; }

        static MaterialPropertyBlock _mpb;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor"); // URP Lit
        static readonly int Color = Shader.PropertyToID("_Color");         // Built-in

        public void Setup(ElementData data)
        {
            Data = data;
            name = $"{data.number:000}_{data.symbol}";
            if (symbolText) symbolText.text = data.symbol;
            if (numberText) numberText.text = data.number.ToString();
            if (nameText) nameText.text = data.name;
            SetColor(CategoryColors.For(data.category));
        }

        public void SetColor(Color c)
        {
            if (!body) return;
            _mpb ??= new MaterialPropertyBlock();
            body.GetPropertyBlock(_mpb);
            _mpb.SetColor(BaseColor, c);
            _mpb.SetColor(Color, c);
            body.SetPropertyBlock(_mpb);
        }
    }
}

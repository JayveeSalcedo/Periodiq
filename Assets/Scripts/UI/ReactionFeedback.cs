using System.Collections.Generic;
using Periodiq.Data;
using Periodiq.Reactions;
using UnityEngine;

namespace Periodiq.UI
{
    /// <summary>Listens to the ReactionZone and fills the info card.</summary>
    public class ReactionFeedback : MonoBehaviour
    {
        [SerializeField] ReactionZone zone;
        [SerializeField] InfoCardUI card;

        [Header("Accent colours")]
        [SerializeField] Color covalentColor = new(0.3f, 0.75f, 1f);
        [SerializeField] Color ionicColor = new(0.7f, 0.5f, 1f);
        [SerializeField] Color failColor = new(1f, 0.55f, 0.3f);
        [SerializeField] Color hintColor = new(1f, 0.85f, 0.3f);

        void OnEnable()
        {
            zone.CompoundFormed += OnFormed;
            zone.ReactionFailed += OnFailed;
        }

        void OnDisable()
        {
            zone.CompoundFormed -= OnFormed;
            zone.ReactionFailed -= OnFailed;
        }

        void OnFormed(CompoundData c, Molecule m)
        {
            string body = $"<i>{c.description}</i>\n\n{ChemistryExplainer.ExplainCompound(c)}";
            card.Show(
                c.name,
                ChemistryExplainer.FormatFormula(c.formula),
                c.IsIonic ? "IONIC" : "COVALENT",
                c.IsIonic ? ionicColor : covalentColor,
                body);
        }

        void OnFailed(IReadOnlyList<ElementData> atoms)
        {
            string text = ChemistryExplainer.ExplainNoReaction(atoms);
            bool isHint = text.StartsWith("Almost") || text.StartsWith("Too many") || text.StartsWith("Close");
            card.Show(
                isHint ? "Almost there" : "No reaction",
                "",
                isHint ? "HINT" : "WHY?",
                isHint ? hintColor : failColor,
                text);
        }
    }
}

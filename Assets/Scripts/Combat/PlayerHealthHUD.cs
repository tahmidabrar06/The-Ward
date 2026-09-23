using UnityEngine;

namespace TheWard.Combat
{
    [RequireComponent(typeof(Health))]
    public class PlayerHealthHUD : MonoBehaviour
    {
        [Header("Health Bar")]
        [SerializeField] private Health health;
        [SerializeField] private Vector2 screenPosition = new Vector2(24f, 24f);
        [SerializeField] private Vector2 barSize = new Vector2(280f, 28f);
        [SerializeField] private Color healthyColor = new Color(0.2f, 0.78f, 0.3f, 1f);
        [SerializeField] private Color dangerColor = new Color(0.85f, 0.12f, 0.12f, 1f);

        [Header("Damage Message")]
        [SerializeField, Min(0.1f)] private float damageMessageDuration = 0.8f;

        private GUIStyle labelStyle;
        private float lastDamage;
        private float damageMessageUntil;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }
        }

        private void OnEnable()
        {
            health.Damaged += ShowDamage;
        }

        private void OnDisable()
        {
            health.Damaged -= ShowDamage;
        }

        private void ShowDamage(float amount)
        {
            lastDamage = amount;
            damageMessageUntil = Time.time + damageMessageDuration;
        }

        private void OnGUI()
        {
            if (health == null)
            {
                return;
            }

            EnsureStyle();

            Rect background = new Rect(screenPosition.x, screenPosition.y, barSize.x, barSize.y);
            GUI.color = new Color(0.03f, 0.03f, 0.03f, 0.88f);
            GUI.DrawTexture(background, Texture2D.whiteTexture);

            float healthPercent = health.NormalizedHealth;
            Rect fill = new Rect(background.x + 3f, background.y + 3f,
                (background.width - 6f) * healthPercent, background.height - 6f);
            GUI.color = Color.Lerp(dangerColor, healthyColor, healthPercent);
            GUI.DrawTexture(fill, Texture2D.whiteTexture);

            GUI.color = Color.white;
            GUI.Label(background,
                $"HEALTH  {Mathf.CeilToInt(health.CurrentHealth)} / {Mathf.CeilToInt(health.MaxHealth)}",
                labelStyle);

            if (Time.time < damageMessageUntil)
            {
                float fade = Mathf.Clamp01((damageMessageUntil - Time.time) / damageMessageDuration);
                GUI.color = new Color(1f, 0.2f, 0.2f, fade);
                GUI.Label(new Rect(background.x, background.yMax + 5f, background.width, 25f),
                    $"-{Mathf.CeilToInt(lastDamage)} DAMAGE", labelStyle);
            }

            GUI.color = Color.white;
        }

        private void EnsureStyle()
        {
            if (labelStyle != null)
            {
                return;
            }

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };
            labelStyle.normal.textColor = Color.white;
        }
    }
}

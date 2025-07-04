using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameByte.Automation;

namespace GameByte.UI
{
    /// <summary>
    /// Oyun için esnek header bileşeni
    /// Score, coin, health ve diğer UI elemanları için kullanılabilir
    /// Unity UI sistemi ile uyumlu olarak tasarlanmıştır
    /// </summary>
    public class GameHeader : MonoBehaviour
    {
        [Header("Header Layout Settings")]
        [AutoLink(searchByName: "LeftContainer")]
        [SerializeField] private RectTransform leftContainer;
        [AutoLink(searchByName: "CenterContainer")]
        [SerializeField] private RectTransform centerContainer;
        [AutoLink(searchByName: "RightContainer")]
        [SerializeField] private RectTransform rightContainer;
        
        [Header("Default Elements")]
        [AutoLink(searchInChildren: true, searchByName: "ScoreText")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [AutoLink(searchInChildren: true, searchByName: "CoinsText")]
        [SerializeField] private TextMeshProUGUI coinsText;
        [AutoLink(searchInChildren: true, searchByName: "HealthText")]
        [SerializeField] private TextMeshProUGUI healthText;
        [AutoLink(searchInChildren: true, searchByName: "CoinIcon")]
        [SerializeField] private Image coinIcon;
        [AutoLink(searchInChildren: true, searchByName: "HealthIcon")]
        [SerializeField] private Image healthIcon;
        
        [Header("Header Configuration")]
        [SerializeField] private bool showScore = true;
        [SerializeField] private bool showCoins = true;
        [SerializeField] private bool showHealth = true;
        [SerializeField] private float headerHeight = 100f;
        [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.3f);
        
        // Game values
        private int currentScore = 0;
        private int currentCoins = 0;
        private int currentHealth = 100;
        private int maxHealth = 100;
        
        private void Start()
        {
            InitializeHeader();
            UpdateDisplay();
        }
        
        /// <summary>
        /// Header bileşenini başlangıç değerleriyle ayarlar
        /// </summary>
        private void InitializeHeader()
        {
            // Header yüksekliğini ayarla
            RectTransform headerRect = GetComponent<RectTransform>();
            if (headerRect != null)
            {
                headerRect.sizeDelta = new Vector2(headerRect.sizeDelta.x, headerHeight);
            }
            
            // Arka plan rengini ayarla
            Image backgroundImage = GetComponent<Image>();
            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
            }
            
            // Elemanları gizle/göster
            ConfigureElements();
        }
        
        /// <summary>
        /// Hangi elemanların gösterileceğini belirler
        /// </summary>
        private void ConfigureElements()
        {
            if (scoreText != null)
                scoreText.gameObject.SetActive(showScore);
                
            if (coinsText != null && coinIcon != null)
            {
                coinsText.gameObject.SetActive(showCoins);
                coinIcon.gameObject.SetActive(showCoins);
            }
            
            if (healthText != null && healthIcon != null)
            {
                healthText.gameObject.SetActive(showHealth);
                healthIcon.gameObject.SetActive(showHealth);
            }
        }
        
        /// <summary>
        /// Tüm UI elemanlarını güncel değerlerle günceller
        /// </summary>
        private void UpdateDisplay()
        {
            UpdateScore();
            UpdateCoins();
            UpdateHealth();
        }
        
        #region Score Management
        /// <summary>
        /// Score değerini günceller
        /// </summary>
        /// <param name="newScore">Yeni score değeri</param>
        public void SetScore(int newScore)
        {
            currentScore = newScore;
            UpdateScore();
        }
        
        /// <summary>
        /// Score'a değer ekler
        /// </summary>
        /// <param name="scoreToAdd">Eklenecek score miktarı</param>
        public void AddScore(int scoreToAdd)
        {
            currentScore += scoreToAdd;
            UpdateScore();
        }
        
        private void UpdateScore()
        {
            if (scoreText != null && showScore)
            {
                scoreText.text = $"Score: {currentScore:N0}";
            }
        }
        #endregion
        
        #region Coin Management
        /// <summary>
        /// Coin değerini günceller
        /// </summary>
        /// <param name="newCoins">Yeni coin değeri</param>
        public void SetCoins(int newCoins)
        {
            currentCoins = newCoins;
            UpdateCoins();
        }
        
        /// <summary>
        /// Coin'e değer ekler
        /// </summary>
        /// <param name="coinsToAdd">Eklenecek coin miktarı</param>
        public void AddCoins(int coinsToAdd)
        {
            currentCoins += coinsToAdd;
            UpdateCoins();
        }
        
        /// <summary>
        /// Coin harcama işlemi
        /// </summary>
        /// <param name="coinsToSpend">Harcanacak coin miktarı</param>
        /// <returns>İşlem başarılı ise true</returns>
        public bool SpendCoins(int coinsToSpend)
        {
            if (currentCoins >= coinsToSpend)
            {
                currentCoins -= coinsToSpend;
                UpdateCoins();
                return true;
            }
            return false;
        }
        
        private void UpdateCoins()
        {
            if (coinsText != null && showCoins)
            {
                coinsText.text = currentCoins.ToString("N0");
            }
        }
        #endregion
        
        #region Health Management
        /// <summary>
        /// Health değerini günceller
        /// </summary>
        /// <param name="newHealth">Yeni health değeri</param>
        public void SetHealth(int newHealth)
        {
            currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
            UpdateHealth();
        }
        
        /// <summary>
        /// Health'e değer ekler (healing)
        /// </summary>
        /// <param name="healthToAdd">Eklenecek health miktarı</param>
        public void AddHealth(int healthToAdd)
        {
            currentHealth = Mathf.Clamp(currentHealth + healthToAdd, 0, maxHealth);
            UpdateHealth();
        }
        
        /// <summary>
        /// Damage alma işlemi
        /// </summary>
        /// <param name="damage">Alınacak damage miktarı</param>
        public void TakeDamage(int damage)
        {
            currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
            UpdateHealth();
        }
        
        /// <summary>
        /// Maksimum health değerini ayarlar
        /// </summary>
        /// <param name="newMaxHealth">Yeni maksimum health değeri</param>
        public void SetMaxHealth(int newMaxHealth)
        {
            maxHealth = newMaxHealth;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UpdateHealth();
        }
        
        private void UpdateHealth()
        {
            if (healthText != null && showHealth)
            {
                healthText.text = $"{currentHealth}/{maxHealth}";
            }
        }
        #endregion
        
        #region Container Access
        /// <summary>
        /// Sol container'a yeni UI elemanı eklemek için referans
        /// </summary>
        public RectTransform LeftContainer => leftContainer;
        
        /// <summary>
        /// Merkez container'a yeni UI elemanı eklemek için referans
        /// </summary>
        public RectTransform CenterContainer => centerContainer;
        
        /// <summary>
        /// Sağ container'a yeni UI elemanı eklemek için referans
        /// </summary>
        public RectTransform RightContainer => rightContainer;
        #endregion
        
        #region Public Getters
        public int CurrentScore => currentScore;
        public int CurrentCoins => currentCoins;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        #endregion
    }
} 
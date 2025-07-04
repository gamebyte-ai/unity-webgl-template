using UnityEngine;
using UnityEngine.InputSystem;
using GameByte.UI;
using GameByte.Automation;

namespace GameByte.Demo
{
    /// <summary>
    /// GameHeader bileşenini test etmek için demo script
    /// Oyun sırasında score, coin ve health değerlerini simüle eder
    /// Yeni Input System kullanır
    /// </summary>
    public class HeaderDemo : MonoBehaviour, InputSystem_Actions.IUIDemoActions
    {
        [Header("Demo Settings")]
        [AutoLink(searchByTag: "UIHeader")]
        [SerializeField] private GameHeader gameHeader;
        [SerializeField] private bool autoDemo = true;
        [SerializeField] private float demoInterval = 2f;
        
        private float nextDemoTime;
        private InputSystem_Actions inputActions;
        
        private void Awake()
        {
            // Input Actions'ı başlat
            inputActions = new InputSystem_Actions();
        }
        
        private void Start()
        {
            if (gameHeader == null)
            {
                gameHeader = FindObjectOfType<GameHeader>();
            }
            
            if (gameHeader == null)
            {
                Debug.LogError("GameHeader bulunamadı! HeaderDemo çalışması için GameHeader gerekli.");
                enabled = false;
                return;
            }
            
            // Başlangıç değerlerini ayarla
            gameHeader.SetScore(0);
            gameHeader.SetCoins(100);
            gameHeader.SetHealth(100);
            gameHeader.SetMaxHealth(100);
            
            nextDemoTime = Time.time + demoInterval;
            
            Debug.Log("Header Demo başlatıldı!");
            Debug.Log("Kontroller: Q = Score ekle, W = Coin ekle, E = Damage al, R = İyileş");
        }
        
        private void OnEnable()
        {
            if (inputActions != null)
            {
                inputActions.UIDemo.SetCallbacks(this);
                inputActions.UIDemo.Enable();
            }
        }
        
        private void OnDisable()
        {
            if (inputActions != null)
            {
                inputActions.UIDemo.Disable();
            }
        }
        
        private void OnDestroy()
        {
            inputActions?.Dispose();
        }
        
        private void Update()
        {
            if (autoDemo && Time.time >= nextDemoTime)
            {
                PerformRandomDemo();
                nextDemoTime = Time.time + demoInterval;
            }
        }
        
        /// <summary>
        /// Otomatik demo için rastgele eylemler gerçekleştirir
        /// </summary>
        private void PerformRandomDemo()
        {
            int action = Random.Range(0, 4);
            
            switch (action)
            {
                case 0:
                    gameHeader.AddScore(Random.Range(50, 200));
                    Debug.Log("Auto Demo: Score eklendi");
                    break;
                    
                case 1:
                    gameHeader.AddCoins(Random.Range(10, 50));
                    Debug.Log("Auto Demo: Coin eklendi");
                    break;
                    
                case 2:
                    gameHeader.TakeDamage(Random.Range(5, 25));
                    Debug.Log("Auto Demo: Damage alındı");
                    break;
                    
                case 3:
                    gameHeader.AddHealth(Random.Range(10, 30));
                    Debug.Log("Auto Demo: İyileşildi");
                    break;
            }
        }
        
        /// <summary>
        /// Demo'yu manual olarak başlatmak/durdurmak için
        /// </summary>
        [System.Obsolete("Use ToggleAutoDemo instead")]
        public void ToggleDemo()
        {
            ToggleAutoDemo();
        }
        
        /// <summary>
        /// Otomatik demo'yu açar/kapatır
        /// </summary>
        public void ToggleAutoDemo()
        {
            autoDemo = !autoDemo;
            Debug.Log($"Auto Demo: {(autoDemo ? "Açık" : "Kapalı")}");
        }
        
        /// <summary>
        /// Tüm değerleri sıfırlar
        /// </summary>
        public void ResetValues()
        {
            gameHeader.SetScore(0);
            gameHeader.SetCoins(100);
            gameHeader.SetHealth(100);
            Debug.Log("Tüm değerler sıfırlandı!");
        }
        
        #region Input Actions Callbacks
        
        public void OnAddScore(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                gameHeader.AddScore(Random.Range(10, 100));
                Debug.Log("Score eklendi!");
            }
        }
        
        public void OnAddCoin(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                gameHeader.AddCoins(Random.Range(5, 25));
                Debug.Log("Coin eklendi!");
            }
        }
        
        public void OnTakeDamage(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                gameHeader.TakeDamage(Random.Range(10, 30));
                Debug.Log("Damage alındı!");
            }
        }
        
        public void OnHeal(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                gameHeader.AddHealth(Random.Range(15, 35));
                Debug.Log("İyileşildi!");
            }
        }
        
        #endregion
    }
} 
# GameByte UI Header System

Unity UI sistemi için tasarlanmış esnek header bileşeni. Archero benzeri oyunlarda kullanım için optimize edilmiştir.

## 🚀 Özellikler

- **Esnek Layout**: Sol, merkez ve sağ container'lar ile tam esneklik
- **Otomatik Yönetim**: Score, coin ve health değerlerini otomatik günceller
- **Responsive Design**: Farklı ekran boyutlarında uyumlu çalışır
- **SOLID Principles**: Kodlar SOLID prensiplerine uygun yazılmıştır
- **Unity UI Uyumlu**: Unity'nin standart UI sistemi ile tam uyumlu

## 📁 Dosya Yapısı

```
Assets/Scripts/UI/
├── GameHeader.cs         # Ana header bileşeni
├── HeaderDemo.cs         # Test ve demo script'i
└── README.md            # Bu dokümantasyon
```

## 🎮 Kullanım

### Basit Kullanım

```csharp
using GameByte.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameHeader gameHeader;
    
    void Start()
    {
        // Başlangıç değerlerini ayarla
        gameHeader.SetScore(0);
        gameHeader.SetCoins(100);
        gameHeader.SetHealth(100);
    }
    
    void OnPlayerScored(int points)
    {
        gameHeader.AddScore(points);
    }
    
    void OnPlayerCollectedCoin(int amount)
    {
        gameHeader.AddCoins(amount);
    }
    
    void OnPlayerTookDamage(int damage)
    {
        gameHeader.TakeDamage(damage);
    }
}
```

### Demo Kontrolleri

HeaderDemo script'i ile test edebilirsiniz:

- **Q**: Score ekle
- **W**: Coin ekle
- **E**: Damage al
- **R**: İyileş

## 🔧 Header Konfigürasyonu

### Inspector Ayarları

- **Show Score**: Score gösterimini açar/kapatır
- **Show Coins**: Coin gösterimini açar/kapatır  
- **Show Health**: Health gösterimini açar/kapatır
- **Header Height**: Header yüksekliği (varsayılan: 100px)
- **Background Color**: Arka plan rengi (varsayılan: transparan siyah)

### Container Erişimi

Header'a yeni elemanlar eklemek için container'lara erişebilirsiniz:

```csharp
// Sol tarafa yeni element ekle
Transform leftArea = gameHeader.LeftContainer;
newUIElement.SetParent(leftArea);

// Merkeze element ekle
Transform centerArea = gameHeader.CenterContainer;
centerUIElement.SetParent(centerArea);

// Sağa element ekle
Transform rightArea = gameHeader.RightContainer;
rightUIElement.SetParent(rightArea);
```

## 📱 Responsive Design

Header sistem, Canvas Scaler ile otomatik olarak responsive çalışır:

- **Reference Resolution**: 430x932 (Mobile)
- **Screen Match Mode**: Match Width Or Height
- **Match**: 0.5 (Balanced)

## 🎨 Stilizasyon

### TextMeshPro Ayarları

- **Font Size**: 18pt
- **Color**: Beyaz
- **Alignment**: Center (Score), Left (Health), Right (Coins)

### Layout Yapısı

```
GameHeader (RectTransform)
├── LeftContainer (33% genişlik)
│   └── HealthText
├── CenterContainer (34% genişlik)
│   └── ScoreText
└── RightContainer (33% genişlik)
    └── CoinsText
```

## 🔄 API Referansı

### Public Methods

#### Score Management
- `SetScore(int newScore)`: Score değerini ayarlar
- `AddScore(int scoreToAdd)`: Score'a değer ekler
- `int CurrentScore`: Mevcut score değeri

#### Coin Management
- `SetCoins(int newCoins)`: Coin değerini ayarlar
- `AddCoins(int coinsToAdd)`: Coin'e değer ekler
- `bool SpendCoins(int coinsToSpend)`: Coin harcama (true/false döner)
- `int CurrentCoins`: Mevcut coin değeri

#### Health Management
- `SetHealth(int newHealth)`: Health değerini ayarlar
- `AddHealth(int healthToAdd)`: Health ekler (iyileşme)
- `TakeDamage(int damage)`: Damage alır
- `SetMaxHealth(int newMaxHealth)`: Maksimum health ayarlar
- `int CurrentHealth`: Mevcut health değeri
- `int MaxHealth`: Maksimum health değeri

### Container Access
- `RectTransform LeftContainer`: Sol container referansı
- `RectTransform CenterContainer`: Merkez container referansı
- `RectTransform RightContainer`: Sağ container referansı

## 🛠 Kurulum

1. **Prefab Kullanımı** (Önerilen):
   - `Assets/Prefabs/GameHeaderPrefab.prefab`'ı sahneye sürükleyin
   - Canvas'a child olarak ekleyin

2. **Manuel Kurulum**:
   - Empty GameObject oluşturun
   - `GameHeader` script'ini ekleyin
   - Gerekli UI elemanlarını oluşturup referansları bağlayın

## 📋 Gereksinimler

- Unity 2022.3 LTS veya üzeri
- TextMeshPro package
- Input System package (demo için)

## 🔍 Troubleshooting

### Sık Karşılaşılan Sorunlar

1. **Text gözükmüyor**: TextMeshPro font asset'inin atandığından emin olun
2. **Layout bozuk**: RectTransform anchor/pivot ayarlarını kontrol edin  
3. **Demo çalışmıyor**: HeaderDemo script'inde GameHeader referansı atandığından emin olun

### Debug Modları

Console'da debug mesajları için HeaderDemo script'ini kullanın:

```csharp
Debug.Log($"Current Score: {gameHeader.CurrentScore}");
Debug.Log($"Current Coins: {gameHeader.CurrentCoins}");
Debug.Log($"Current Health: {gameHeader.CurrentHealth}");
```

## 📈 Performans

- **Draw Call Optimized**: UI Atlas kullanımı önerilir
- **Memory Efficient**: String allocation minimum seviyede
- **Update Optimized**: Sadece değer değişiminde güncelleme

## 🎯 Gelecek Güncellemeler

- [ ] Icon desteği (coin/health)
- [ ] Animasyon sistemi
- [ ] Sound effect entegrasyonu
- [ ] Customizable theme sistem
- [ ] Progress bar elemanları

## 👥 Katkıda Bulunma

Bu sistem GameByte projesi için geliştirilmiştir. Öneri ve iyileştirmeler için proje repository'sini kullanın.

---

**Not**: Bu sistem Unity'nin resmi UI dokümantasyonuna uygun olarak geliştirilmiştir ve production projelerinde kullanıma hazırdır. 
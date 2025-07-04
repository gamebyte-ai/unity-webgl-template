#!/bin/bash

# Unity Batch Mode Automation Script
# GameByte Unity Project için otomatik script linking ve prefab binding

# Renk kodları
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Unity executable path (macOS)
UNITY_PATH="/Applications/Unity/Hub/Editor/2023.2.0f1/Unity.app/Contents/MacOS/Unity"

# Proje path'i (script'in çalıştırıldığı yerden bir üst klasör)
PROJECT_PATH=$(cd "$(dirname "$0")/.." && pwd)

# Log dosyası
LOG_FILE="$PROJECT_PATH/automation.log"

echo -e "${BLUE}🚀 Unity GameByte Automation Script${NC}"
echo -e "Project Path: ${PROJECT_PATH}"
echo -e "Unity Path: ${UNITY_PATH}"
echo ""

# Unity'nin varlığını kontrol et
if [ ! -f "$UNITY_PATH" ]; then
    echo -e "${RED}❌ Unity bulunamadı: $UNITY_PATH${NC}"
    echo -e "${YELLOW}💡 Unity path'ini script içinde düzenleyin${NC}"
    exit 1
fi

# Fonksiyon: Unity batch mode komut çalıştır
run_unity_command() {
    local method=$1
    local description=$2
    
    echo -e "${YELLOW}⚙️  $description...${NC}"
    
    "$UNITY_PATH" \
        -batchmode \
        -quit \
        -projectPath "$PROJECT_PATH" \
        -executeMethod "$method" \
        -logFile "$LOG_FILE" \
        -nographics
    
    local exit_code=$?
    
    if [ $exit_code -eq 0 ]; then
        echo -e "${GREEN}✅ $description başarılı${NC}"
    else
        echo -e "${RED}❌ $description başarısız (Exit code: $exit_code)${NC}"
        echo -e "${YELLOW}📄 Log dosyasını kontrol edin: $LOG_FILE${NC}"
        return $exit_code
    fi
}

# Ana menü
show_menu() {
    echo -e "${BLUE}📋 Automation Seçenekleri:${NC}"
    echo "1. 🔗 Tüm Script Referanslarını Bağla"
    echo "2. 🎮 GameHeader'ları Konfigüre Et"
    echo "3. 🖼️  UI Component'larını Optimize Et"
    echo "4. 📦 Prefab'ları İşle"
    echo "5. 🔧 Scene Objelerini Optimize Et"
    echo "6. 🚀 Full Automation (Hepsi)"
    echo "7. 🏗️  Build WebGL"
    echo "8. 🏗️  Build WebGL (Full Automation ile)"
    echo "9. 🏗️  Build WebGL (Development)"
    echo "10. 🧹 Cache Temizle"
    echo "11. 📄 Log Dosyasını Göster"
    echo "0. ❌ Çıkış"
    echo ""
}

# Ana döngü
while true; do
    show_menu
    read -p "Seçiminizi yapın (0-11): " choice
    echo ""
    
    case $choice in
        1)
            run_unity_command "BatchModeAutomation.LinkAllScriptReferences" "Script referansları bağlanıyor"
            ;;
        2)
            run_unity_command "BatchModeAutomation.ConfigureGameHeaders" "GameHeader'lar konfigüre ediliyor"
            ;;
        3)
            run_unity_command "BatchModeAutomation.ConfigureUIComponents" "UI component'ları optimize ediliyor"
            ;;
        4)
            run_unity_command "BatchModeAutomation.ProcessAllPrefabs" "Prefab'lar işleniyor"
            ;;
        5)
            run_unity_command "BatchModeAutomation.OptimizeSceneObjects" "Scene objeleri optimize ediliyor"
            ;;
        6)
            run_unity_command "BatchModeAutomation.AutomateAllInspectorOperations" "Full automation çalıştırılıyor"
            ;;
        7)
            run_unity_command "BuildScript.BuildWebGL" "WebGL build başlatılıyor"
            ;;
        8)
            run_unity_command "BuildScript.BuildWebGLWithFullAutomation" "WebGL build (Full Automation) başlatılıyor"
            ;;
        9)
            run_unity_command "BuildScript.BuildWebGLDevelopment" "WebGL Development build başlatılıyor"
            ;;
        10)
            run_unity_command "BatchModeAutomation.ClearCache" "Cache temizleniyor"
            ;;
        11)
            if [ -f "$LOG_FILE" ]; then
                echo -e "${BLUE}📄 Log Dosyası İçeriği:${NC}"
                echo "================================"
                tail -50 "$LOG_FILE"
                echo "================================"
            else
                echo -e "${YELLOW}⚠️  Log dosyası bulunamadı: $LOG_FILE${NC}"
            fi
            ;;
        0)
            echo -e "${GREEN}👋 Automation script sonlandırıldı${NC}"
            exit 0
            ;;
        *)
            echo -e "${RED}❌ Geçersiz seçim: $choice${NC}"
            ;;
    esac
    
    echo ""
    read -p "Devam etmek için Enter'a basın..."
    echo ""
done 
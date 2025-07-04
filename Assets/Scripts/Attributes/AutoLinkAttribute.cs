using System;

namespace GameByte.Automation
{
    /// <summary>
    /// Unity batch mode'da otomatik script referansı bağlama için kullanılan attribute
    /// SerializedField ile birlikte kullanılarak component referanslarını otomatik bağlar
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class AutoLinkAttribute : Attribute
    {
        /// <summary>
        /// Parent GameObject'te arama yapar
        /// </summary>
        public bool searchInParent;
        
        /// <summary>
        /// Child GameObject'lerde arama yapar
        /// </summary>
        public bool searchInChildren;
        
        /// <summary>
        /// Belirli bir component tipini aramak için tag kullanır
        /// </summary>
        public string searchByTag;
        
        /// <summary>
        /// GameObject adına göre arama yapar
        /// </summary>
        public string searchByName;
        
        /// <summary>
        /// AutoLink attribute constructor
        /// </summary>
        /// <param name="searchInParent">Parent'ta ara</param>
        /// <param name="searchInChildren">Child'larda ara</param>
        /// <param name="searchByTag">Tag'e göre ara</param>
        /// <param name="searchByName">İsme göre ara</param>
        public AutoLinkAttribute(bool searchInParent = false, bool searchInChildren = false, 
                                string searchByTag = "", string searchByName = "")
        {
            this.searchInParent = searchInParent;
            this.searchInChildren = searchInChildren;
            this.searchByTag = searchByTag;
            this.searchByName = searchByName;
        }
    }
} 
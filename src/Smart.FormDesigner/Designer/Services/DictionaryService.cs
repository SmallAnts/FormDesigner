using System.Collections;
using System.ComponentModel.Design;

namespace Smart.FormDesigner.Services
{
    public class DictionaryService : AbstractService, IDictionaryService
    {
        private IDictionary dictionary;

        public DictionaryService()
        {
            this.dictionary = new Hashtable();
        }

        public object GetValue(object key)
        {
            return this.dictionary[key];
        }

        public void SetValue(object key, object value)
        {
            this.dictionary[key] = value;
        }

        public object GetKey(object value)
        {
            foreach (DictionaryEntry dictionaryEntry in this.dictionary)
            {
                if (dictionaryEntry.Value == value)
                {
                    return dictionaryEntry.Key;
                }
            }
            return null;
        }

    }
}

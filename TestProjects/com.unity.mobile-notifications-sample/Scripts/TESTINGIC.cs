using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.UI;

#if UNITY_ANDROID || UNITY_EDITOR
using Unity.Notifications.Android;
#endif

namespace Unity.Notifications.Tests.Sample
{
    public class TESTINGIC : MonoBehaviour
    {
        InterfaceConstructor ic;
        
        // Start is called before the first frame update
        void Start()
        {
            InterfaceConstructor ic = gameObject.GetComponent<InterfaceConstructor>();
            GameObject go = ic.createCanvas();
            
            go = ic.createPanel(200, 200, go.transform);
            ic.AddLayoutGroup(go, false);

            PropertyInfo[] properties = typeof(AndroidNotification).GetProperties();
            GameObject propertyGO;
            foreach (PropertyInfo property in properties)
            {
                //property.SetValue(record, value);
                propertyGO = ic.createPanel(200, 200, go.transform);
                ic.AddLayoutGroup(propertyGO, true);
                propertyGO.name=property.Name;
                ic.createText(property.Name, propertyGO.transform);

               // print(property.PropertyType.ToString()); 
                switch (property.PropertyType.ToString()){
                    case "System.String":
                        ic.createInputField(InputField.ContentType.IntegerNumber, propertyGO.transform);
                        break;
                    case "System.Boolean":
                        ic.createToggle(20, propertyGO.transform);                    
                        break;
                }
             
            }
           // Button btn=ic.createButton("Confirm notification changes", go.transform);
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}

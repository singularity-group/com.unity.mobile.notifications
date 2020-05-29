using UnityEngine;
using UnityEngine.UI;

namespace Unity.Notifications.Tests.Sample
{
    public class InterfaceConstructor : MonoBehaviour
    {
        [SerializeField] private GameObject gameObjectInstance;
        [SerializeField] Sprite defaultSprite;
        [SerializeField] Sprite spriteCheckMark;
        private GameObject go;

        void Awake(){
           
        }


        public void AddComponent(GameObject go, string componentToAdd){

        }

        #region Modifications
            public GameObject instantiation(string nameGO, Transform parentGO=null){
                go = GameObject.Instantiate(gameObjectInstance, parentGO);
                go.layer=5;
                go.name=nameGO;
                if (parentGO!=null){
                    go.transform.parent=parentGO;
                }
                go.transform.localScale=Vector2.one;
                go.transform.localPosition=Vector3.zero;
                return go;
            }

            private void setPivotPoints(RectTransform rt){
                rt.anchorMin = new Vector2(0, 0);
                    rt.anchorMax = new Vector2(1, 1);
                    rt.pivot = new Vector2(0.5f, 0.5f);
            }

        #endregion

        #region Components
            public void AddSprite(GameObject go, Sprite spr){
                Image img = go.AddComponent<Image>();
                img.sprite=spr;
                img.type = Image.Type.Sliced;
            }

            public void AddLayoutGroup(GameObject go, bool horizontal){
                if (horizontal){
                    go.AddComponent<HorizontalLayoutGroup>();
                } else {
                    go.AddComponent<VerticalLayoutGroup>();
                }
                //go.GetComponent<VerticalLayoutGroup>();
            }

            public void AddText(GameObject go, string textField){
                Text txt =go.AddComponent<Text>();
                txt.font=Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                txt.fontSize=15;
                txt.text=textField;
                txt.color=Color.black;
                txt.alignment = TextAnchor.MiddleCenter;
            }


        #endregion

        #region UIobjects
            public GameObject createCanvas(Transform parentGO=null){
                if (GameObject.Find("EventSystem")==null){
                    // create event system
                }
                go = instantiation("Canvas", parentGO);
                Canvas cnv = go.AddComponent<Canvas>();
                cnv.renderMode=RenderMode.ScreenSpaceOverlay;
                go.AddComponent<CanvasScaler>();
                CanvasScaler cs =  go.GetComponent<CanvasScaler>();
                cs.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
                go.AddComponent<GraphicRaycaster>();
                return go;
            }
            
            public GameObject createText(string textField, Transform parentGO=null){
                go=instantiation("TextField", parentGO);
                AddText(go, textField);
                return go;
            }

            public GameObject createToggle(float size, Transform parentGO=null){
                GameObject goParent=instantiation("ToggleButton", parentGO);
                Toggle tgl = goParent.AddComponent<Toggle>();
                go=instantiation("Background", go.transform);
                AddSprite(go, defaultSprite);
                RectTransform rc = go.GetComponent<RectTransform>();
                print("AFTER INVOKE");
               // rc.sizeDelta = (parentXY.x > parentXY.y) ? parentXY=new Vector2(parentXY.y,parentXY.y) : parentXY=new Vector2(parentXY.x,parentXY.x);
               rc.sizeDelta=new Vector2(size,size);
                tgl.targetGraphic=go.GetComponent<Image>();
                go=instantiation("Checkmark", go.transform);
                AddSprite(go, spriteCheckMark);
                go.GetComponent<RectTransform>().sizeDelta=new Vector2(size,size);
                tgl.graphic = go.GetComponent<Image>();
                return go;
            }

            public GameObject createPanel(int w, int h, Transform parentGO=null){
                go=instantiation("Panel", parentGO);
                RectTransform rt = go.AddComponent<RectTransform>();
                //rt.SetAsLastSibling();
                setPivotPoints(rt);
                rt.sizeDelta=new Vector2(-h,-w);
                AddSprite(go, defaultSprite);
                return go;
            }

            public GameObject createButton(string buttonText, Transform parentGO=null){
                GameObject hierarchyParent=instantiation("Button", parentGO);
                AddSprite(go, defaultSprite);
                Button btn = go.AddComponent<Button>();
                AddLayoutGroup(go,true);
                btn.targetGraphic = go.GetComponent<Image>();

                go=instantiation("ButtonText", go.transform);
                AddText(go, buttonText);
                return hierarchyParent;
            }

            public GameObject createInputField(InputField.ContentType ct, Transform parentGO=null){
                go=instantiation("InputField", parentGO);
                AddSprite(go, defaultSprite);
                InputField inp = go.AddComponent<InputField>();
                inp.contentType=ct;
                inp.targetGraphic = go.GetComponent<Image>();

                GameObject childGo=instantiation("Placeholder", go.transform);
                //childGo.transform.SetAsLastSibling();
                AddText(childGo, "Enter text...");
                inp.placeholder=childGo.GetComponent<Text>();

                childGo=instantiation("Text", go.transform);
                //childGo.transform.SetAsLastSibling();
                AddText(childGo, "");
                inp.textComponent=childGo.GetComponent<Text>();
                return childGo;
            }
        #endregion

    }
}


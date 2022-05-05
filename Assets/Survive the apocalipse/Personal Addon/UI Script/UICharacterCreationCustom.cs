using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using TMPro;
using Mirror;
using System.Linq;
using AdvancedPeopleSystem;

public class UICharacterCreationCustom : MonoBehaviour
{
    public CharacterCustomization maleCharacterCustomization;
    public CharacterCustomization femaleCharacterCustomization;

    public CharacterGeneratorSettings maleCharacterGeneratorSettings;
    public CharacterGeneratorSettings femaleCharacterGeneratorSettings;

    public CharacterGeneratorSettings desiredCharacterGeneratorSettings;

    public CharacterCustomization desiredCharacterCustomization;

    public static UICharacterCreationCustom singleton;
    public NetworkManagerMMO manager;

    public SpawnManager spawnManager;

    public TMP_InputField nameInput;
    public GameObject pickColorPanel;
    public ColorPick colorPicker;



    public int selectedSex;

    public Button createButton;
    public Button cancelButton;

    public UICharacterSelection selection;

    public Player[] playerPreview;

    public RawImage creationImage;

    public bool existsPlayer;

    public Slider sliderFat;
    public Slider sliderThin;
    public Slider sliderMuscle;
    public Slider sliderHeight;
    public TextMeshProUGUI breastTxt;
    public Slider sliderBreast;

    public Button male;
    public Button female;

    public Button skin1;
    public Button skin2;
    public Button skin3;
    public Button skin4;
    public Button skin5;
    public Button skin6;

    public Button hairType;
    public Button hairColor;

    public TextMeshProUGUI beardTxt;
    public TextMeshProUGUI thinTxt;
    public TextMeshProUGUI muscleTxt;
    public Button beard;

    public Button eyesColor;

    public Button underwearColor;

    public int typeOfPerson;

    public bool inHairColorSelection;
    public bool inEyesColorSelection;
    public bool inUnderwearColorSelection;

    #region general

    public int GeneralSex;

    public int HairIndex;
    public int BeardIndex;

    public string HairColor;
    public string UnderwearColor;
    public string EyesColor;
    public string SkinColor;

    public float FatAmount;
    public float ThinAmount;
    public float MuscleAmount;
    public float HeightAmount;
    public float BreastAmount = 0.0f;

    #endregion

    #region man
    public int manHairIndex;
    public int manBeardIndex;

    public string manHairColor;
    public string manUnderwearColor;
    public string manEyesColor;
    public string manSkinColor;

    public float manFatAmount;
    public float manThinAmount;
    public float manMuscleAmount;
    public float manHeightAmount;
    public float manBreastAmount = 0.0f;

    #endregion

    #region woman
    public int femaleHairIndex;
    public int femaleBeardIndex;

    public string femeleHairColor;
    public string femaleUnderwearColor;
    public string femaleEyesColor;
    public string femaleSkinColor;

    public float femaleFatAmount;
    public float femaleThinAmount;
    public float femaleMuscleAmount;
    public float femaleHeightAmount;
    public float femaleBreastAmount;

    #endregion

    void Start()
    {
        if (!singleton) singleton = this;
        selection = FindObjectOfType<UICharacterSelection>();
        manager = FindObjectOfType<NetworkManagerMMO>();

        SetMaleCharacter();

        createButton.onClick.SetListener(() =>
        {

            for (int i = 0; i < playerPreview.Length; i++)
            {
                if (playerPreview[i].gameObject.name == nameInput.text)
                {
                    Instantiate(GeneralManager.singleton.popUpAutomicaticName, GeneralManager.singleton.canvas);
                    return;

                }
            }

            if (selectedSex == 0)
            {
                GeneralSex = selectedSex;
                HairIndex = manHairIndex;
                BeardIndex = manBeardIndex;
                HairColor = manHairColor;
                UnderwearColor = manUnderwearColor;
                EyesColor = manEyesColor;
                SkinColor = manSkinColor;
                FatAmount = manFatAmount;
                ThinAmount = manThinAmount;
                MuscleAmount = manMuscleAmount;
                HeightAmount = manHeightAmount;
                BreastAmount = 0;
            }
            else
            {
                GeneralSex = selectedSex;
                HairIndex = femaleHairIndex;
                BeardIndex = femaleBeardIndex;
                HairColor = femeleHairColor;
                UnderwearColor = femaleUnderwearColor;
                EyesColor = femaleEyesColor;
                SkinColor = femaleSkinColor;
                FatAmount = femaleFatAmount;
                ThinAmount = femaleThinAmount;
                MuscleAmount = 0;
                HeightAmount = femaleHeightAmount;
                BreastAmount = femaleBreastAmount;
            }

            CharacterCreateMsg message = new CharacterCreateMsg
            {
                name = nameInput.text,
                classIndex = 1,
                sex = GeneralSex,
                skinColor = SkinColor,
                hairColor = HairColor,
                hairType = HairIndex,
                eyesColor = EyesColor,
                beard = BeardIndex,
                underwearColor = UnderwearColor,
                fat = FatAmount,
                thin = ThinAmount,
                muscle = MuscleAmount,
                height = HeightAmount,
                breast = BreastAmount
            };
            NetworkClient.Send(message);
            pickColorPanel.gameObject.SetActive(false);
            cancelButton.onClick.Invoke();
        });

        cancelButton.onClick.SetListener(() =>
        {
            UICharacterSelection characterSelection = FindObjectOfType<UICharacterSelection>();
            if (characterSelection)
            {
                characterSelection.mainCamera.depth = 1;
                characterSelection.creationCamera.depth = 0;
            }
            ResetCharacterCreation();
        });

        male.onClick.SetListener(() =>
        {
            SetMaleCharacter();
        });
        female.onClick.SetListener(() =>
        {
            SetFemaleCharacter();
        });

        skin1.onClick.SetListener(() =>
        {
            Color color = skin1.image.color;
            ChangeColorSkin(color);
        });
        skin2.onClick.SetListener(() =>
        {
            Color color = skin2.image.color;
            ChangeColorSkin(color);
        });
        skin3.onClick.SetListener(() =>
        {
            Color color = skin3.image.color;
            ChangeColorSkin(color);
        });
        skin4.onClick.SetListener(() =>
        {
            Color color = skin4.image.color;
            ChangeColorSkin(color);
        });
        skin5.onClick.SetListener(() =>
        {
            Color color = skin5.image.color;
            ChangeColorSkin(color);
        });
        skin6.onClick.SetListener(() =>
        {
            Color color = skin6.image.color;
            ChangeColorSkin(color);
        });

        hairType.onClick.SetListener(() =>
        {
            ChangeTyperHair();
        });

        hairColor.onClick.SetListener(() =>
        {
            if (!inEyesColorSelection && !inUnderwearColorSelection)
            {
                pickColorPanel.gameObject.SetActive(!pickColorPanel.activeInHierarchy);
            }
            else
            {
                pickColorPanel.gameObject.SetActive(true);
            }
            inHairColorSelection = true;
            inEyesColorSelection = false;
            inUnderwearColorSelection = false;
        });

        eyesColor.onClick.SetListener(() =>
        {
            if (!inHairColorSelection && !inUnderwearColorSelection)
            {
                pickColorPanel.gameObject.SetActive(!pickColorPanel.activeInHierarchy);
            }
            else
            {
                pickColorPanel.gameObject.SetActive(true);
            }
            inHairColorSelection = false;
            inEyesColorSelection = true;
            inUnderwearColorSelection = false;
        });

        underwearColor.onClick.SetListener(() =>
        {
            if (!inHairColorSelection && !inEyesColorSelection)
            {
                pickColorPanel.gameObject.SetActive(!pickColorPanel.activeInHierarchy);
            }
            else
            {
                pickColorPanel.gameObject.SetActive(true);
            }
            inHairColorSelection = false;
            inEyesColorSelection = false;
            inUnderwearColorSelection = true;
        });

        beard.onClick.SetListener(() =>
        {
            ChangeBread();
        });
    }


    void Update()
    {
        ChangeColor();
        createButton.interactable = manager.IsAllowedCharacterName(nameInput.text) && selectedSex != -1;
    }


    public void ChangeColor()
    {
        if(inHairColorSelection == true)
        {
            ChangeColorHair(colorPicker.pickedColor);
        }
        else if (inEyesColorSelection == true)
        {
            ChangeColorEyes(colorPicker.pickedColor);
        }
        else if (inUnderwearColorSelection == true)
        {
            ChangeColorUnderwear(colorPicker.pickedColor);
        }
    }

    public void ResetCharacterCreation()
    {
        nameInput.text = string.Empty;


        selection.creationPanel.SetActive(false);
        selection.creationColor.SetActive(false);
        pickColorPanel.gameObject.SetActive(false);

    }

    public void SetMaleCharacter()
    {
        selectedSex = 0;
        desiredCharacterCustomization = maleCharacterCustomization;
        desiredCharacterGeneratorSettings = maleCharacterGeneratorSettings;
        maleCharacterCustomization.gameObject.SetActive(true);
        femaleCharacterCustomization.gameObject.SetActive(false);
        beard.gameObject.SetActive(true);

        beardTxt.gameObject.SetActive(true);
        sliderBreast.gameObject.SetActive(false);
        breastTxt.gameObject.SetActive(false);
        thinTxt.gameObject.SetActive(true);
        sliderThin.gameObject.SetActive(true);
        muscleTxt.gameObject.SetActive(true);
        sliderMuscle.gameObject.SetActive(true);

        typeOfPerson = 0;
        sliderFat.value = manFatAmount;
        sliderBreast.value = manBreastAmount;
        sliderHeight.value = manHeightAmount;
        sliderMuscle.value = manMuscleAmount;
        sliderThin.value = manThinAmount;

        pickColorPanel.gameObject.SetActive(false);
    }

    public void SetFemaleCharacter()
    {
        selectedSex = 1;
        desiredCharacterCustomization = femaleCharacterCustomization;
        desiredCharacterGeneratorSettings = femaleCharacterGeneratorSettings;
        maleCharacterCustomization.gameObject.SetActive(false);
        femaleCharacterCustomization.gameObject.SetActive(true);

        beard.gameObject.SetActive(false);
        beardTxt.gameObject.SetActive(false);
        sliderBreast.gameObject.SetActive(true);
        breastTxt.gameObject.SetActive(true);
        thinTxt.gameObject.SetActive(false);
        sliderThin.gameObject.SetActive(false);
        muscleTxt.gameObject.SetActive(false);
        sliderMuscle.gameObject.SetActive(false);

        typeOfPerson = 1;
        sliderFat.value = femaleFatAmount;
        sliderBreast.value = femaleBreastAmount;
        sliderHeight.value = femaleHeightAmount;
        sliderMuscle.value = femaleMuscleAmount;
        sliderThin.value = femaleThinAmount;

        pickColorPanel.gameObject.SetActive(false);

    }

    public void ChangeColorSkin(Color color)
    {
        if (selectedSex == 0)
            manSkinColor = "#" + ColorUtility.ToHtmlStringRGB(color);
        else
            femaleSkinColor = "#" + ColorUtility.ToHtmlStringRGB(color);
        desiredCharacterCustomization.SetBodyColor(BodyColorPart.Skin, color);
    }

    public void ChangeTyperHair()
    {
        if (selectedSex == 0)
        {
            manHairIndex++;
            if (manHairIndex > desiredCharacterCustomization.Settings.hairPresets.Count - 1) manHairIndex = -1;
            desiredCharacterCustomization.SetElementByIndex(CharacterElementType.Hair, manHairIndex);
        }
        else
        {
            femaleHairIndex++;
            if (femaleHairIndex > desiredCharacterCustomization.Settings.hairPresets.Count - 1) femaleHairIndex = -1;
            desiredCharacterCustomization.SetElementByIndex(CharacterElementType.Hair, femaleHairIndex);
        }
    }

    public void ChangeColorHair(Color color)
    {
        if (selectedSex == 0)
            manHairColor = "#" + ColorUtility.ToHtmlStringRGB(color);
        else
            femeleHairColor = "#" + ColorUtility.ToHtmlStringRGB(color);
        desiredCharacterCustomization.SetBodyColor(BodyColorPart.Hair, color);
    }


    public void ChangeColorEyes(Color color)
    {
        if (selectedSex == 0)
            manEyesColor = "#" + ColorUtility.ToHtmlStringRGB(color);
        else
            femaleEyesColor = "#" + ColorUtility.ToHtmlStringRGB(color);
        desiredCharacterCustomization.SetBodyColor(BodyColorPart.Eye, color);
    }

    public void ChangeColorUnderwear(Color color)
    {
        if (selectedSex == 0)
            manUnderwearColor = "#" + ColorUtility.ToHtmlStringRGB(color);
        else
            femaleUnderwearColor = "#" + ColorUtility.ToHtmlStringRGB(color);
        desiredCharacterCustomization.SetBodyColor(BodyColorPart.Underpants, color);
    }

    public void ChangeBread()
    {
        if (selectedSex == 0)
        {
            manBeardIndex++;
            if (manBeardIndex > desiredCharacterCustomization.Settings.beardPresets.Count - 1) manBeardIndex = -1;
            desiredCharacterCustomization.SetElementByIndex(CharacterElementType.Beard, manBeardIndex);
        }
    }

    public void ChangeBodyForm()
    {

        if (selectedSex == 0)
        {
            manFatAmount = sliderFat.value;
            desiredCharacterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Fat, sliderFat.value);
        }
        else
        {
            femaleFatAmount = sliderFat.value;
            desiredCharacterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Fat, sliderFat.value);
        }

        if (selectedSex == 0)
        {
            manThinAmount = sliderThin.value;
            desiredCharacterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Thin, sliderThin.value);
        }
        else
        {
            femaleThinAmount = sliderThin.value;
        }

        if (selectedSex == 0)
        {
            manMuscleAmount = sliderMuscle.value;
            desiredCharacterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Muscles, sliderMuscle.value);
        }
        else
        {
            femaleMuscleAmount = sliderMuscle.value;
        }

        if (selectedSex == 0)
        {
            manHeightAmount = sliderHeight.value;
            desiredCharacterCustomization.SetHeight(sliderHeight.value);
        }
        else
        {
            femaleHeightAmount = sliderHeight.value;
            desiredCharacterCustomization.SetHeight(sliderHeight.value);
        }

        if (selectedSex == 0)
        {
            manBreastAmount = sliderBreast.value;
        }
        else
        {
            femaleBreastAmount = sliderBreast.value;
            desiredCharacterCustomization.SetBlendshapeValue(CharacterBlendShapeType.BreastSize, sliderBreast.value);
        }
    }

}

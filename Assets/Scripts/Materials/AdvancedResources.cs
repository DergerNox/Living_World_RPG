using System.Collections.Generic;
using UnityEngine;

public class AdvancedResources
{
    //Public Variables
    public string Name { get{return name;}set{name = value;}}
    public AdvancedResourcesTypes AdvancedResourceType { get{return advancedResourceType;}set{advancedResourceType = value;}}
    public Resources InputResourceA { get{return inputResourceA;}set{inputResourceA = value;}}
    public Resources InputResourceB { get{return inputResourceB;}set{inputResourceB = value;}}
    public int MaxInputResourceA { get{return maxInputResourceA;}set{maxInputResourceA = value;}}
    public int MinInputResourceA { get{return minInputResourceA;}set{minInputResourceA = value;}}
    public int CurrentAmountOfInputResourceA { get{return currentAmountOfInputResourceA;}set{currentAmountOfInputResourceA = value;}}
    public int MaxInputResourceB { get{return maxInputResourceB;}set{maxInputResourceB = value;}}
    public int MinInputResourceB { get{return minInputResourceB;}set{minInputResourceB = value;}}
    public int CurrentAmountOfInputResourceB { get{return currentAmountOfInputResourceB;}set{currentAmountOfInputResourceB = value;}}
    public int AmountOfAdvancedResources { get{return amountOfAdvancedResources;}set{amountOfAdvancedResources = value;}}
    //Constructor
    public AdvancedResources(AdvancedResourcesTypes i_advancedResourceType,int i_workshopLevel)
    {
        name=i_advancedResourceType.ToString();
        advancedResourceType = i_advancedResourceType;
        amountOfAdvancedResources = 0;
        switch(i_workshopLevel)
        {
            case 1:
                minInputResourceA=2;
                maxInputResourceA=50;
                minInputResourceB=2;
                maxInputResourceB=50;
                break;
            case 2:
                minInputResourceA=2;
                maxInputResourceA=100;
                minInputResourceB=2;
                maxInputResourceB=100;
                break;
            case 3:
                minInputResourceA=2;
                maxInputResourceA=200;
                minInputResourceB=2;
                maxInputResourceB=200;
                break;
        }
    }
    //Public Methods
    public void MakeAdvancedResource()
    {
       if (CurrentAmountOfInputResourceA>=MinInputResourceA&&CurrentAmountOfInputResourceB>=MinInputResourceB)
       {
        currentAmountOfInputResourceA -= MinInputResourceA;
        currentAmountOfInputResourceB -= MinInputResourceB;
        AmountOfAdvancedResources += 1;
       }
       else
       {
           Debug.Log("Not enough resources to make advanced resource");
       }
    }
    //private Variables
    private string name;
    private AdvancedResourcesTypes advancedResourceType;
    private int amountOfAdvancedResources;
    private Resources inputResourceA;
    private int maxInputResourceA;
    private int minInputResourceA;
    private int currentAmountOfInputResourceA;
    private Resources inputResourceB;
    private int maxInputResourceB;
    private int minInputResourceB;
    private int currentAmountOfInputResourceB;
    
}

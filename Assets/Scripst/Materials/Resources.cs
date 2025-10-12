using Mono.Cecil;
using UnityEngine;

public class Resources
{
    //public variables
    public int ResourceAmount{get{return resourceAmount;}set{resourceAmount = value;}}
    public ResourceType ResourceType{get{return resourceType;}set{resourceType = value;}}
    public string ResourceName{get{return resourceName;}set{resourceName = value;}}
    //Constructor
    public Resources(int i_amount, ResourceType i_type)
    {
        resourceAmount = i_amount;
        resourceType = i_type;
        resourceName = resourceType.ToString();
    }
    //private variables
    private int resourceAmount;
    private ResourceType resourceType;
    private string resourceName;
}

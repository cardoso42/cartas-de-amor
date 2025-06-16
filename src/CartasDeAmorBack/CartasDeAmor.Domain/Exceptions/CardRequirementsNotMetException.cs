using System;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Exceptions;

/// <summary>
/// Exception thrown when a card's requirements are not met
/// </summary>
[Serializable]
public class CardRequirementsNotMetException : CardPlayException
{
    public CardActionRequirements? RequirementType { get; }
    
    public CardRequirementsNotMetException(string message) : base(message) { }
    
    public CardRequirementsNotMetException(string message, CardType cardType) : base(message, cardType) { }
    
    public CardRequirementsNotMetException(string message, CardType cardType, CardActionRequirements requirementType) 
        : base(message, cardType) 
    {
        RequirementType = requirementType;
    }
    
    public CardRequirementsNotMetException(string message, Exception innerException) : base(message, innerException) { }
}

using System.ComponentModel.DataAnnotations;

namespace Core.Helpers;

public class DecimalRangeAttribute : ValidationAttribute
{
    private string? _minNumber;
    private string? _maxNumber;
    private bool _isMinIncluded;
    private bool _isMaxIncluded;
    
    public DecimalRangeAttribute(string? minNumber, string? maxNumber, bool isMinIncluded = false, bool isMaxIncluded = false)
    {
        _minNumber = minNumber;
        _maxNumber = maxNumber;
        _isMinIncluded = isMinIncluded;
        _isMaxIncluded = isMaxIncluded;
    }
    
    public override bool IsValid(object? value)
    {
        if (value == null) return false;

        var number = value as decimal?;
        
        decimal minNumber;
        if (!decimal.TryParse(_minNumber, out minNumber)) return false;
        
        decimal maxNumber;
        if (!decimal.TryParse(_maxNumber, out maxNumber)) return false;
        
        this.EnsureLegalLengths(minNumber,maxNumber);

        // if (string.IsNullOrEmpty(_minNumber))
        // {
        //     switch (_isMinIncluded, _isMaxIncluded)
        //     {
        //         case (true, true):
        //             if (number >= minNumber && number <= maxNumber) return true;
        //             break;
        //     }
        // }
        
        switch (_isMinIncluded,_isMaxIncluded)
        {
            case (true,true):
                if (number >= minNumber && number <= maxNumber) return true;
                break;
            case (true,false):
                if (number >= minNumber && number < maxNumber) return true;
                break;
            case (false,true):
                if (number > minNumber && number <= maxNumber) return true;
                break;
            case (false,false):
                if (number > minNumber && number < maxNumber) return true;
                break;
        }
        
        return false;
    }
    
    public override string FormatErrorMessage(string name)
    {
        return string.Format(this.ErrorMessageString, name);
    }

    private void EnsureLegalLengths(decimal minNumber, decimal maxNumber)
    {
        if (minNumber > maxNumber)
            throw new InvalidOperationException($"{nameof(minNumber)} can't be more than {nameof(maxNumber)}");
    }
}
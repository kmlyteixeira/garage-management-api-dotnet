using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Values;

public class Document : ValueObject
{
    public string Value { get; private set; }

    public Document(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Document cannot be empty");

        value = OnlyValidCharacters(value);

        if (!IsValid(value))
            throw new ArgumentException("Invalid CPF/CNPJ");

        Value = value;
    }

    /// <summary>
    /// Removes all invalid characters from the input, considering alfa-numeric characters.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private string OnlyValidCharacters(string input)
        => Regex.Replace(input, "[^0-9a-zA-Z]", "");

    private bool IsValid(string value)
    {
        return value.Length == 11 || value.Length == 14;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
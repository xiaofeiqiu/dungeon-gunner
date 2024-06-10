using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helper
{
    /// <summary>
    /// Checks if a string is empty and logs an error if it is.
    /// </summary>
    /// <param name="thisObject">The object containing the field being checked.</param>
    /// <param name="fieldName">The name of the field being checked.</param>
    /// <param name="stringToCheck">The string value to check.</param>
    /// <returns>True if the string is empty, false otherwise.</returns>
    public static bool ValidateCheckEmptyString(Object thisObject, string fieldName, string stringToCheck)
    {
        if (stringToCheck == "")
        {
            Debug.Log(fieldName + " is empty and must contain a value in object " + thisObject.name.ToString());
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if an enumerable contains null values or is empty and logs an error if it does.
    /// </summary>
    /// <param name="thisObject">The object containing the field being checked.</param>
    /// <param name="fieldName">The name of the field being checked.</param>
    /// <param name="enumerableObjectToCheck">The enumerable to check.</param>
    /// <returns>True if the enumerable contains null values or is empty, false otherwise.</returns>
    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName, IEnumerable enumerableObjectToCheck)
    {
        bool error = false;
        int count = 0;

        foreach (var item in enumerableObjectToCheck)
        {
            if (item == null)
            {
                Debug.Log(fieldName + " has null values in object " + thisObject.name.ToString());
                error = true;
            }
            else
            {
                count++;
            }
        }

        if (count == 0)
        {
            Debug.Log(fieldName + " has no values in object " + thisObject.name.ToString());
            error = true;
        }

        return error;
    }
}

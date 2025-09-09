using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NumberExtensions
{
    public static string ToMoney(this double number)
    {
        string[] suffixes = { "", "K", "M", "B", "T" };
        int suffixIndex = 0;
        double shortNumber = number;

        while (shortNumber >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            shortNumber /= 1000;
            suffixIndex++;
        }

        return $"{shortNumber:0.##}{suffixes[suffixIndex]}";
    }

    public static string ToMoney(this long number)
    {
        string[] suffixes = { "", "K", "M", "B", "T" };
        int suffixIndex = 0;
        long shortNumber = number;

        while (shortNumber >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            shortNumber /= 1000;
            suffixIndex++;
        }

        return $"{shortNumber:0.##}{suffixes[suffixIndex]}";
    }

    public static string ToRank(this int number)
    {
        if (number <= 0) return number.ToString();

        int lastTwoDigits = number % 100;
        if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
        {
            return number + "th";
        }

        switch (number % 10)
        {
            case 1:
                return number + "st";
            case 2:
                return number + "nd";
            case 3:
                return number + "rd";
            default:
                return number + "th";
        }
    }
}

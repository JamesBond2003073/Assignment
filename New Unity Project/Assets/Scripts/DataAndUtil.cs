using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RepositoryData
{
    public string rank;
    public string username;
    public string repositoryName;
    public string url;
    public string description;
    public string language;
    public string languageColor;
    public string totalStars;
    public string forks;
    public string StarsSince;
    public string since;
    public List<BuiltBy> builtBy;

}

public class BuiltBy
{
    public string username;
    public string url;
    public string avatar;
}

public class Joke
{
    public string value;
}

public static class Util
{
    public static float GetPercent(float input, float min, float max)
    {
        return ((input - min) * 100) / (max - min);
    }

    public static List<GameObject> starredItems = new List<GameObject>();
}
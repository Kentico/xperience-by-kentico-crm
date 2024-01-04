﻿namespace Kentico.Xperience.CRM.Common.Configuration;

/// <summary>
/// Common setting for Kentico-CRM integration
/// </summary>
/// <typeparam name="TApiConfig"></typeparam>
public class CommonIntegrationSettings<TApiConfig>
{
    public bool FormLeadsEnabled { get; set; }
    // @TODO phase 2
    public bool ContactsEnabled { get; set; }

    public TApiConfig? ApiConfig { get; set; }
}
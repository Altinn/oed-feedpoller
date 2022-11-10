using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oed_feedpoller;
public static class Constants
{
    public const string DaHttpClient = "DaHttpClient";
    public const string EventsHttpClient = "EventsHttpClient";

    public const string SchemaDodsfallsak = "https://schemas.domstol.no/doedsfallsak.schema.json";
    public const string EventTypeDodsfallsak = "doedsfallsak";

    public const string SchemaFormuesfullmakt = "https://schemas.domstol.no/formuesfullmakt.schema.json";
    public const string EventTypeFormuesfullmakt = "formuesfullmakt";

    public const string RoleDeceased = "avdoed";

}

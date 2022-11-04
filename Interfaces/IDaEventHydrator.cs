using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using oed_feedpoller.Models;

namespace oed_feedpoller.Interfaces;
public interface IDaEventHydrator
{
    public Task<DaEvent> GetHydratedEvent(string eventJson);
}

using oed_feedpoller.Models.Da;
using oed_feedpoller.Models.Da.Dto;

namespace oed_feedpoller.Interfaces;

public interface IMapperFactory
{
    IDaEventMapper? GetMapper(string eventType);
}
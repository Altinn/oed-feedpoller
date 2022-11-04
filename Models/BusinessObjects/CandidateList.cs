using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using oed_feedpoller.Interfaces;

namespace oed_feedpoller.Models.BusinessObjects;
public class CandidateList : IBusinessObject
{
    public List<Candidate> Candidates { get; set; } = new();
}

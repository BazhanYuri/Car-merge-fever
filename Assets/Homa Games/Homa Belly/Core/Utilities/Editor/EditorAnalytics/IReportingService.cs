using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public interface IReportingService
    {
        event System.Action<EventApiQueryModel> OnDataReported;
    }
}

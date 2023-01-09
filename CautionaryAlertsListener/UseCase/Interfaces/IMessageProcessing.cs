using Hackney.Core.Sns;
using System.Threading.Tasks;

namespace CautionaryAlertsListener.UseCase.Interfaces
{
    public interface IMessageProcessing
    {
        Task ProcessMessageAsync(EntityEventSns message);
    }
}

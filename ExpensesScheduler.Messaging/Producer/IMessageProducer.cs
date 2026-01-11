using System;
using System.Collections.Generic;
using System.Text;

namespace ExpensesScheduler.Messaging.Producer;

public interface IMessageProducer<in Messsage> : IDisposable 
{
    Task ProduceAsync(Messsage messsage, CancellationToken ct);

}

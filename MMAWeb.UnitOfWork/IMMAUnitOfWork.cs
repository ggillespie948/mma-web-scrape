using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MMAWeb.UnitOfWork
{
    public interface IMMAUnitOfWork
    {
        void Complete();
        Task CompleteAsync();
    }
}

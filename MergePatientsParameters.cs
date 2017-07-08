namespace Eyefinity.PracticeManagement.Model.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class MergePatientsParameters
    {
        public int Patient1Id { get; set; }

        public int Patient2Id { get; set; }

        public int SelectedPatient { get; set; }
    }
}

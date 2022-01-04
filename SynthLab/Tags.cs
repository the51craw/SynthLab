using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthLab
{
    public enum TagType
    {
        OSCILLATOR,
        FILTER,
        ADSR,
        WIRE,
        OTHER
    }

    public class BaseTag
    {
        public int Id { get { return id; } }
        public TagType TagType { get { return tagType; } }
        public int ParentId;

        public MainPage mainPage;

        protected int id;
        protected TagType tagType;

        public BaseTag(int Id, int ParentId = -1)
        {
            tagType = TagType.OTHER;
            id = Id;
            this.ParentId = ParentId;
        }
    }

    public class OscillatorTag : BaseTag
    {
        public int OscillatorId;
        public int OscillatorRow { get { return oscillatorRow; } }
        public int OscillatorColumn { get { return oscillatorColumn; } }
        public int ControlNumber { get { return controlNumber; } }

        private int oscillatorRow;
        private int oscillatorColumn;
        private int controlNumber;

        public OscillatorTag(MainPage mainPage, int Id, int OscillatorRow, int OscillatorColumn, int ControlNumber) : base (Id)
        {
            base.mainPage = mainPage;
            tagType = TagType.OSCILLATOR;
            oscillatorRow = OscillatorRow;
            oscillatorColumn = OscillatorColumn;
            OscillatorId = oscillatorRow * mainPage.MainLayout.Columns + oscillatorColumn;
            controlNumber = ControlNumber;
        }
    }

    public class FilterTag : BaseTag
    {
        public int OscillatorId;
        public int OscillatorRow { get { return oscillatorRow; } }
        public int OscillatorColumn { get { return oscillatorColumn; } }
        public int ControlNumber { get { return controlNumber; } }

        private int oscillatorRow;
        private int oscillatorColumn;
        private int controlNumber;

        public FilterTag(MainPage mainPage, int Id, int OscillatorRow, int OscillatorColumn, int ControlNumber) : base(Id)
        {
            base.mainPage = mainPage;
            tagType = TagType.FILTER;
            oscillatorRow = OscillatorRow;
            oscillatorColumn = OscillatorColumn;
            OscillatorId = oscillatorRow * mainPage.MainLayout.Columns + oscillatorColumn;
            controlNumber = ControlNumber;
        }
    }

    public class AdsrTag : BaseTag
    {
        public int OscillatorId;
        public int OscillatorRow { get { return oscillatorRow; } }
        public int OscillatorColumn { get { return oscillatorColumn; } }
        public int ControlNumber { get { return controlNumber; } }

        private int oscillatorRow;
        private int oscillatorColumn;
        private int controlNumber;

        public AdsrTag(MainPage mainPage, int Id, int OscillatorRow, int OscillatorColumn, int ControlNumber) : base(Id)
        {
            base.mainPage = mainPage;
            tagType = TagType.ADSR;
            oscillatorRow = OscillatorRow;
            oscillatorColumn = OscillatorColumn;
            OscillatorId = oscillatorRow * mainPage.MainLayout.Columns + oscillatorColumn;
            controlNumber = ControlNumber;
        }
    }

    public class WireTag : BaseTag
    {
        public int WireNumber;

        public WireTag(int Id, int WireNumber) : base(Id)
        {
            this.WireNumber = WireNumber;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReadDataBaseService
{
    public class T_SCALE_DATA
    {
        private string fD_GOODS_NAME;

        public string FD_GOODS_NAME
        {
            get
            {
                return fD_GOODS_NAME;
            }

            set
            {
                fD_GOODS_NAME = value;
            }
        }

        public string FD_NET_WEIGHT
        {
            get
            {
                return fD_NET_WEIGHT;
            }

            set
            {
                fD_NET_WEIGHT = value;
            }
        }

        public string FD_UPLOAD_DATE
        {
            get
            {
                return fD_UPLOAD_DATE;
            }

            set
            {
                fD_UPLOAD_DATE = value;
            }
        }

        private string fD_NET_WEIGHT;

        private string fD_UPLOAD_DATE;
    }
}

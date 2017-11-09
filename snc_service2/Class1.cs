using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snc_service
{
    public static class Class1
    {
        static void Main()
        {
            using (var snc = new snc_service())
            {
                if (!snc.Open("localhost", 9584))
                    return;

                string sResult = "";
                //Чтение информации по карте
                var tt = snc.ReadCard(SNC_CARD_TYPE.SNC_CARD_TYPE_FUEL, 3531, true, out sResult);

                //Дебетование карты 1 литр стоимостью 2 рубля. 
                snc.DebitCard(3531, 1000, 200, 2, 3000000100000485, false, out sResult);
                
                //Возврат 1.5 рублей на карту
                snc.RepaymentCard(3531, 0, 150, 2, 3000000100000485, true, out sResult);
                //Возврат 0.25 л на карту
                snc.RepaymentCard(3531, 250, 20, 2, 3000000100000485, true, out sResult);
            }
        }
    }
}

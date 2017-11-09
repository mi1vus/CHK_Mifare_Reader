using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Class1
{
    static void Main()
    {
        using (var snc = new snc_service())
        {
            if (!snc.Open("127.0.0.1", 5002))
                return;

            string sResult = "";
            int pin = 7839;
            SNC_SERVICE_RESULT res = snc.ReadCard(SNC_CARD_TYPE.SNC_CARD_TYPE_FUEL, pin, false, out sResult);
            if (res == SNC_SERVICE_RESULT.SNC_SERVICE_RESULT_SUCCESS && !string.IsNullOrWhiteSpace(sResult))
            {
                var card = snc.ParseCardInfo(sResult);
                //res = snc.CloseShift();
                //res = snc.OpenShift();
                Console.WriteLine("Card read success! Debit?");
                var с = Console.ReadLine();
                ////Дебетование карты 2.5 литр стоимостью 5 рубля. 
                res = snc.DebitCard(pin, 2000, 2000, card.Products.First().ProdгctCode, card.CardNumber, true, out sResult);
                Console.WriteLine("Card Debit success! Repayment?");
                с = Console.ReadLine();
                ////Возврат 1.5 рублей на карту
                //snc.RepaymentCard(pin, 0, 150, 2, 3000000100000485, true, out sResult);
                ////Возврат 0.25 л на карту
                //snc.RepaymentCard(pin, 250, 20, 2, 3000000100000485, true, out sResult);
                res = snc.RepaymentCard(pin, 950, 950, card.Products.First().ProdгctCode, card.CardNumber, true, out sResult);
                Console.WriteLine("Card Repayment success! Repayment?");
                с = Console.ReadLine();
                res = snc.RepaymentCard(pin, 1050, 1050, card.Products.First().ProdгctCode, card.CardNumber, true, out sResult);
            }
            /*
                res = snc.DebitCard(pin, 2500, 500, card.Products.First().ProdгctCode, card.CardNumber, true, out sResult);
                res = snc.RepaymentCard(pin, 1, card.Products.First().ProdгctCode, card.CardNumber, true, out sResult);
                res = snc.RepaymentCard(pin, 1, card.Products.First().ProdгctCode, card.CardNumber, true, out sResult);

                */
            //snc.Close();


            //if (!snc.Open("localhost", 9584))
            //    return;

            //string sResult = "";
            ////Чтение информации по карте
            //var tt = snc.ReadCard(SNC_CARD_TYPE.SNC_CARD_TYPE_FUEL, pin, true, out sResult);

            ////Дебетование карты 1 литр стоимостью 2 рубля. 
            //snc.DebitCard(pin, 1000, 200, 2, 3000000100000485, false, out sResult);

            ////Возврат 1.5 рублей на карту
            //snc.RepaymentCard(pin, 0, 150, 2, 3000000100000485, true, out sResult);
            ////Возврат 0.25 л на карту
            //snc.RepaymentCard(pin, 250, 20, 2, 3000000100000485, true, out sResult);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming


public enum SNC_SERVICE_RESULT
{
    SNC_SERVICE_RESULT_SUCCESS = 0,             // Успешное выполнение
    SNC_SERVICE_RESULT_ERROR_COMMAND_NOT_SUPPORTED,     // Команда не поддерживается
    SNC_SERVICE_RESULT_ERROR_INVALID_PARAMETER_VALUE,   // Неверное значение параметра
    SNC_SERVICE_RESULT_ERROR_OPERATION_FAILED,      // Ошибка выполнения операции
    SNC_SERVICE_RESULT_ERROR_OPEN_FAILED,           // Ошибка открытия
    SNC_SERVICE_RESULT_ERROR_APPLICATION_FAILED,        // Приложение не найдено
    SNC_SERVICE_RESULT_ERROR_REQUIRED_PIN,          // Необходмо ввести ПИН
};


enum SNC_SERVICE_COMMAND
{
    SNC_SERVICE_COMMAND_READ_CARD = 1,      // Чтение карты
    SNC_SERVICE_COMMAND_DEBIT_CARD = 2,     // Дебетование карты
    SNC_SERVICE_COMMAND_REPAYMENT_CARD = 3,     // Возврат на карту
    SNC_SERVICE_COMMAND_OPEN_SHIFT = 4,     // Открытие смены
    SNC_SERVICE_COMMAND_CLOSE_SHIFT = 5,        // Закрытие смены
};

public enum SNC_CARD_TYPE
{
    SNC_CARD_TYPE_FUEL,
    SNC_CARD_TYPE_DISCOUNT
}

public enum PURSE_TYPE
{
    PURSE_TYPE_VOLUME = 0,
    PURSE_TYPE_PRICE = 1,
    PURSE_TYPE_BONUS = 2,
}

public class snc_service : IDisposable
{
    [DllImport("snc_term.dll", CharSet = CharSet.Ansi)]
    private extern static SNC_SERVICE_RESULT snc_open(char[] pParam);

    [DllImport("snc_term.dll", CharSet = CharSet.Ansi)]
    private extern static SNC_SERVICE_RESULT snc_close();

    [DllImport("snc_term.dll", CharSet = CharSet.Ansi)]
    private extern static SNC_SERVICE_RESULT snc_command(SNC_SERVICE_COMMAND nCommand, char[] pInputData, int nInputSize, IntPtr pOutputData, int nOutputSize, out int pnReturnBytes);


    public bool Open(string host, int port)
    {
        var sInput = "Host=" + host + "\r\nPort=" + port + "\r\n";
        return snc_open(sInput.ToArray()) == SNC_SERVICE_RESULT.SNC_SERVICE_RESULT_SUCCESS;
    }

    public bool Close()
    {
        return snc_close() == SNC_SERVICE_RESULT.SNC_SERVICE_RESULT_SUCCESS;
    }

    public SNC_SERVICE_RESULT ReadCard(SNC_CARD_TYPE nType, int nPin, bool bPrint, out string sResult)
    {
        var sInput = "Type=" + (int)nType + "\r\nPin=" + nPin + "\r\nPrint=" + Convert.ToInt32(bPrint);
        return SncCommand(SNC_SERVICE_COMMAND.SNC_SERVICE_COMMAND_READ_CARD, sInput, out sResult);
    }

    public SNC_SERVICE_RESULT DebitCard(int nPin, int nAmount, int nCost, int nResource, long nGraphicalNumber, bool bPrint, out string sResult)
    {
        var sInput = "Pin=" + nPin + "\r\nPrint=" + Convert.ToInt32(bPrint) + "\r\nAmount=" + nAmount + "\r\nCost=" + nCost
            + "\r\nResourceKey=" + nResource + "\r\nCardNumber=" + nGraphicalNumber + "\r\n";
        return SncCommand(SNC_SERVICE_COMMAND.SNC_SERVICE_COMMAND_DEBIT_CARD, sInput, out sResult);
    }

    public SNC_SERVICE_RESULT RepaymentCard(int nPin, int nAmount, int nCost, int nResource, long nGraphicalNumber, bool bPrint, out string sResult)
    {
        var sInput = "Pin=" + nPin + "\r\nPrint=" + Convert.ToInt32(bPrint) + "\r\nAmount=" + nAmount + "\r\nResourceKey=" + nResource
            + "\r\nCardNumber=" + nGraphicalNumber + "\r\nCost=" + nCost + "\r\n";
        return SncCommand(SNC_SERVICE_COMMAND.SNC_SERVICE_COMMAND_REPAYMENT_CARD, sInput, out sResult);
    }

    public SNC_SERVICE_RESULT OpenShift()
    {
        var sResult = "";
        return SncCommand(SNC_SERVICE_COMMAND.SNC_SERVICE_COMMAND_OPEN_SHIFT, "", out sResult);
    }

    public SNC_SERVICE_RESULT CloseShift()
    {
        var sResult = "";
        return SncCommand(SNC_SERVICE_COMMAND.SNC_SERVICE_COMMAND_CLOSE_SHIFT, "", out sResult);
    }

    private SNC_SERVICE_RESULT SncCommand(SNC_SERVICE_COMMAND nCommand, string sInput, out string sResult)
    {
        int nReturnBytes = 0;
        sResult = "";
        var pOutput = Marshal.AllocHGlobal(65535);
        try
        {
            return snc_command(nCommand, sInput.ToArray(), sInput.Length, pOutput, 65535, out nReturnBytes);
        }
        catch
        {
            nReturnBytes = 0;
            return SNC_SERVICE_RESULT.SNC_SERVICE_RESULT_ERROR_OPERATION_FAILED;
        }
        finally
        {
            if (nReturnBytes > 0)
                sResult = Marshal.PtrToStringAnsi(pOutput);
            Marshal.FreeHGlobal(pOutput);
        }
    }
    public Card ParseCardInfo(string cardInfo)
    {
        var result = new Card();
        var lines = cardInfo.Trim().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            var option = line.Trim().Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
            if (option.Length > 0 && !(option[0].Trim().StartsWith("CardInfo") && !option[0].Trim().StartsWith("CardInfoCount")))
                switch (option[0].Trim())
                {
                    case "CardNumber":
                        result.CardNumber = long.Parse(option[1].Trim());
                        break;
                    case "IssuerCode":
                        result.IssuerCode = int.Parse(option[1].Trim());
                        break;
                    case "OrganizationCode":
                        result.OrganizationCode = int.Parse(option[1].Trim());
                        break;
                    case "PersonCode":
                        result.PersonCode = int.Parse(option[1].Trim());
                        break;
                    case "CardInfoCount":
                        result.CardInfoCount = int.Parse(option[1].Trim());
                        break;
                }
            else
            {
                var par = option[1].Trim().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                result.Products.Add(new Product
                {
                    AppCode = int.Parse(par[0]),
                    PurseTupe = (PURSE_TYPE)int.Parse(par[1]),
                    ProdгctCode = int.Parse(par[2]),
                    Amount = decimal.Parse(par[3]) / 1000,
                    DiscountPercent = float.Parse(par[4]) / 100,
                    DiscountOnOneProdгct = float.Parse(par[5]) / 100,
                    PriceOnOneProdгct = float.Parse(par[6]) / 100
                });
            }
        }
        return result;
    }

    public void Dispose()
    {
        Close();
    }
}
public class Card
{
    public Card()
    {
        Products = new List<Product>();
    }

    public long CardNumber;
    public int IssuerCode;
    public int OrganizationCode;
    public int PersonCode;
    public int CardInfoCount;
    public List<Product> Products;
}
public class Product
{
    public int AppCode;
    public PURSE_TYPE PurseTupe;
    public int ProdгctCode;
    public decimal Amount;
    public float DiscountPercent;
    public float DiscountOnOneProdгct;
    public float PriceOnOneProdгct;
}
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace snc_service
{
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

        public void Dispose()
        {
            Close();
        }
    }
}

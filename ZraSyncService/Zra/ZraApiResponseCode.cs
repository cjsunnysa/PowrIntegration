namespace ZraSyncService.Zra;

public static class ZraResponseCode
{
    // Success Codes
    public static readonly string SUCCESS = "000"; // It is succeeded
    public static readonly string NO_SEARCH_RESULT = "001"; // There is no search result

    // Client Errors
    public static readonly string NO_DATA_TO_RETRANSMIT = "801";
    public static readonly string DATA_NOT_TRANSFERRED = "802";
    public static readonly string REPORT_TRANSFER_COMPLETE = "803";
    public static readonly string NO_DATA_FOR_REPORT = "804";
    public static readonly string RETRANSMISSION_DATA_EXISTS = "805";
    public static readonly string INVALID_SALE_RECEIPT_TYPE = "834";
    public static readonly string SEQUENCE_ALTERED = "836";
    public static readonly string CONNECTION_ERROR = "838";
    public static readonly string INVALID_CUSTOMER_TPIN = "884";
    public static readonly string REQUEST_URL_ERROR = "891";
    public static readonly string REQUEST_HEADER_ERROR = "892";
    public static readonly string REQUEST_BODY_ERROR = "893";
    public static readonly string SERVER_COMMUNICATION_ERROR = "894";
    public static readonly string INVALID_REQUEST_METHOD = "895";
    public static readonly string INVALID_REQUEST_STATUS = "896";
    public static readonly string CLIENT_ERROR = "899";

    // Server Errors
    public static readonly string NO_HEADER_INFORMATION = "900";
    public static readonly string INVALID_DEVICE = "901";
    public static readonly string DEVICE_ALREADY_INSTALLED = "902";
    public static readonly string DEVICE_VERIFICATION_ERROR = "903";
    public static readonly string REQUEST_PARAMETER_ERROR = "910";
    public static readonly string NO_REQUEST_FULL_TEXT = "911";
    public static readonly string REQUEST_METHOD_ERROR = "912";
    public static readonly string CODE_VALUE_ERROR = "913";
    public static readonly string DECLARED_SALES_DATA_ERROR = "921";
    public static readonly string SALES_DATA_REQUIRED = "922";
    public static readonly string DUPLICATE_CIS_INVOICE = "924";
    public static readonly string INVOICE_NOT_FOUND = "930";
    public static readonly string CREDIT_AMOUNT_EXCEEDS_ORIGINAL = "931";
    public static readonly string ITEM_NOT_IN_ORIGINAL_INVOICE = "932";
    public static readonly string CREDIT_NOTE_QUANTITY_EXCEEDED = "934";
    public static readonly string CREDIT_NOTE_MISMATCH = "935";
    public static readonly string MAXIMUM_VIEWS_EXCEEDED = "990";
    public static readonly string REGISTRATION_ERROR = "991";
    public static readonly string MODIFICATION_ERROR = "992";
    public static readonly string DELETION_ERROR = "993";
    public static readonly string OVERLAPPED_DATA_ERROR = "994";
    public static readonly string NO_DOWNLOADED_FILE = "995";
    public static readonly string UNKNOWN_ERROR = "999";
}

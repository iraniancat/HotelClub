namespace HotelReservation.Application.DTOs.Booking;

public class BookingFileDownloadDto
{
    public byte[] FileContent { get; set; }
    public string ContentType { get; set; }
    public string FileName { get; set; }

    public BookingFileDownloadDto(byte[] fileContent, string contentType, string fileName)
    {
        FileContent = fileContent;
        ContentType = contentType;
        FileName = fileName;
    }
}
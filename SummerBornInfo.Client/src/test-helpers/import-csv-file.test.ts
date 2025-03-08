export const getHeaderRow = () => {
  const headers =
    '"LA (code)","LA (name)",' +
    '"TypeOfEstablishment (code)","TypeOfEstablishment (name)",' +
    '"EstablishmentTypeGroup (code)","EstablishmentTypeGroup (name)",' +
    '"EstablishmentStatus (code)","EstablishmentStatus (name)",' +
    '"PhaseOfEducation (code)","PhaseOfEducation (name)",' +
    '"URN","UKPRN",' +
    '"EstablishmentName","EstablishmentNumber",' +
    '"OpenDate","CloseDate",' +
    '"Street","Locality",' +
    '"Address3","Town",' +
    '"County (name)","PostCode"\r\n';
  return headers;
};

export interface DataRow {
  laCode: string;
  laName: string;
  typeCode: string;
  typeName: string;
  groupCode: string;
  groupName: string;
  statusCode: string;
  statusName: string;
  phaseCode: string;
  phaseName: string;
  urn: string;
  ukprn: string;
  establishmentName: string;
  establishmentNumber: string;
  openDate: string;
  closeDate: string;
  street: string;
  locality: string;
  addressThree: string;
  town: string;
  county: string;
  postcode: string;
}

export const getDataRow = (data: DataRow) => {
  const headers =
    `"${data.laCode}","${data.laName}",` +
    `"${data.typeCode}","${data.typeName}",` +
    `"${data.groupCode}","${data.groupName}",` +
    `"${data.statusCode}","${data.statusName}",` +
    `"${data.phaseCode}","${data.phaseName}",` +
    `"${data.urn}","${data.ukprn}",` +
    `"${data.establishmentName}","${data.establishmentNumber}",` +
    `"${data.openDate}","${data.closeDate}",` +
    `"${data.street}","${data.locality}",` +
    `"${data.addressThree}","${data.town}",` +
    `"${data.county}","${data.postcode}"\r\n`;
  return headers;
};

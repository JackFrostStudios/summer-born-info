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
  const row =
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
  return row;
};

export const getValidRowData = (): DataRow => {
  return {
    laCode: '01',
    laName: 'laName',
    typeCode: '02',
    typeName: 'typeName',
    groupCode: '03',
    groupName: 'groupName',
    statusCode: '04',
    statusName: 'statusName',
    phaseCode: '05',
    phaseName: 'phaseName',
    urn: '1',
    ukprn: '2',
    establishmentName: 'establishmentName',
    establishmentNumber: '3',
    openDate: '2020-03-30T12:00:00',
    closeDate: '2021-03-30T12:00:00',
    street: 'street',
    locality: 'locality',
    addressThree: 'addressThree',
    town: 'town',
    county: 'county',
    postcode: 'postcode',
  };
};

export const getValidCsvFile = () => {
  const dataRow = getValidRowData();
  return getCsvFile([dataRow]);
};

export const getCsvFileWithValidationErrors = () => {
  const dataRow = getValidRowData();
  dataRow.ukprn = 'invalidukprn';
  return getCsvFile([dataRow]);
};

export const getInvalidCsvFile = () => {
  const data = '"delimiter" error"';
  const blob = new Blob([data], { type: 'text/csv' });
  return new File([blob], 'import.csv', { type: 'text/csv' });
};

export const getCsvFile = (rows: DataRow[]) => {
  let data = '';
  data += getHeaderRow();
  data += rows.map(r => getDataRow(r)).join('');
  const blob = new Blob([data], { type: 'text/csv' });
  return new File([blob], 'import.csv', { type: 'text/csv' });
};

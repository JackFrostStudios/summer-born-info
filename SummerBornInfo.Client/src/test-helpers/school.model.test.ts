import { ImportSchool } from '@entities';

export const getTestImportSchool = (): ImportSchool => {
  return {
    urn: 1,
    ukprn: 2,
    establishmentNumber: 3,
    name: 'Mock School',
    address: {
      street: 'Street One',
      locality: 'Locality Two',
      addressThree: 'Address Three',
      county: 'County Four',
      town: 'Town Five',
      postCode: 'PC Six',
    },
    openDate: new Date('2020-03-30T12:00:00'),
    closeDate: new Date('2021-03-30T12:00:00'),
    establishmentGroupCode: 'egc1',
    establishmentStatusCode: 'esc2',
    establishmentTypeCode: 'etc3',
    localAuthorityCode: 'lac4',
    phaseOfEducationCode: 'poec5',
  };
};

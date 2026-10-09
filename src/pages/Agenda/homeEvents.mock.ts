import type { HomeEventSection } from './types';

export const HOME_EVENT_SECTIONS: HomeEventSection[] = [
  {
    id: 'today',
    title: 'Hoje',
    events: [
      {
        id: 'brazil-spain-football',
        sport: 'Futebol',
        stage: 'Amistoso',
        time: '21:00',
        venue: 'Estádio Nacional',
        location: 'Brasília - DF',
        homeParticipant: {
          countryName: 'Brasil',
        },
        awayParticipant: {
          countryName: 'Espanha',
        },
      },
    ],
  },
  {
    id: 'tomorrow',
    title: 'Amanhã',
    events: [
      {
        id: 'brazil-japan-volleyball',
        sport: 'Vôlei',
        stage: 'Liga das Nações',
        time: '18:30',
        venue: 'Maracanãzinho',
        location: 'Rio de Janeiro - RJ',
        homeParticipant: {
          countryName: 'Brasil',
        },
        awayParticipant: {
          countryName: 'Japão',
        },
      },
    ],
  },
];

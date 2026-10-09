import type { Edge } from 'react-native-safe-area-context';

export type CountryParticipant = {
  countryName: string;
};

export type HomeEvent = {
  id: string;
  sport: string;
  stage: string;
  time: string;
  venue: string;
  location: string;
  homeParticipant: CountryParticipant;
  awayParticipant: CountryParticipant;
};

export type HomeEventSection = {
  id: string;
  title: string;
  events: HomeEvent[];
};

export const SAFE_AREA_EDGES: Edge[] = ['top', 'left', 'right'];

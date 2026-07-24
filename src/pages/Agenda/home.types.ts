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

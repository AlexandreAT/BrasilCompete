export type CountryFlagProps = {
  countryName: string;
  size?: number;
};

export type FlagContainerProps = {
  $size: number;
};

export const DEFAULT_FLAG_SIZE = 64;

export const FALLBACK_ICON_SCALE = 0.42;

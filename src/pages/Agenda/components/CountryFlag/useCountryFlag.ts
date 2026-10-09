import { useEffect, useState } from 'react';

import { getCountryFlagUrl } from '@/utils/getCountryFlagUrl';

import { type CountryFlagProps, DEFAULT_FLAG_SIZE, FALLBACK_ICON_SCALE } from './types';

export function useCountryFlag({ countryName, size = DEFAULT_FLAG_SIZE }: CountryFlagProps) {
  const flagUrl = getCountryFlagUrl(countryName);
  const [hasImageError, setHasImageError] = useState(false);

  useEffect(() => {
    setHasImageError(false);
  }, [flagUrl]);

  const flagSource = flagUrl && !hasImageError ? { uri: flagUrl } : null;
  const accessibilityLabel = flagSource
    ? `Bandeira de ${countryName}`
    : `Bandeira de ${countryName} indisponível`;
  const fallbackIconSize = Math.round(size * FALLBACK_ICON_SCALE);

  function handleImageError() {
    setHasImageError(true);
  }

  return { accessibilityLabel, fallbackIconSize, flagSource, handleImageError, size };
}

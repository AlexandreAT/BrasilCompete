const COUNTRY_ISO_CODES: Record<string, string> = {
  brasil: 'br',
  brazil: 'br',
  espanha: 'es',
  spain: 'es',
  'estados unidos': 'us',
  'estados unidos da america': 'us',
  japao: 'jp',
  japan: 'jp',
  'united states': 'us',
  'united states of america': 'us',
};

function normalizeCountryName(countryName: string) {
  return countryName
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .trim()
    .toLowerCase();
}

export function getCountryIsoCode(countryName: string) {
  const normalizedCountryName = normalizeCountryName(countryName);

  return COUNTRY_ISO_CODES[normalizedCountryName] ?? null;
}

export function getCountryFlagUrl(countryName: string, width = 160) {
  const isoCode = getCountryIsoCode(countryName);

  return isoCode ? `https://flagcdn.com/w${width}/${isoCode}.png` : null;
}

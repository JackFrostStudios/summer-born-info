type LocalizeFunction = (
  messageParts: TemplateStringsArray,
  ...expressions: readonly unknown[]
) => string;

declare global {
  var $localize: LocalizeFunction | undefined;
}

const stripMetadataBlock = (messagePart: string): string => {
  if (!messagePart.startsWith(':')) {
    return messagePart;
  }

  for (let index = 1; index < messagePart.length; index += 1) {
    if (messagePart[index] === ':' && messagePart[index - 1] !== '\\') {
      return messagePart.slice(index + 1);
    }
  }

  return messagePart;
};

const fallbackLocalize: LocalizeFunction = (messageParts, ...expressions) => {
  let message = stripMetadataBlock(messageParts[0] ?? '');

  for (let index = 0; index < expressions.length; index += 1) {
    message += String(expressions[index]);
    message += stripMetadataBlock(messageParts[index + 1] ?? '');
  }

  return message;
};

globalThis.$localize ??= fallbackLocalize;

export {};

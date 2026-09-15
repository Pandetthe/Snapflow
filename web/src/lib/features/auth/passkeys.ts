export type PasskeyJson = Record<string, unknown>;

export interface PasskeyOptions {
  options: PasskeyJson;
  state: string;
}

type PublicKeyCredentialJsonParsers = typeof PublicKeyCredential & {
  parseCreationOptionsFromJSON?: (options: PasskeyJson) => PublicKeyCredentialCreationOptions;
  parseRequestOptionsFromJSON?: (options: PasskeyJson) => PublicKeyCredentialRequestOptions;
  isConditionalMediationAvailable?: () => Promise<boolean>;
};

type CredentialDescriptorJson = { id: string } & Record<string, unknown>;

function credentialApi(): PublicKeyCredentialJsonParsers {
  return PublicKeyCredential as PublicKeyCredentialJsonParsers;
}

function toBuffer(value: string): ArrayBuffer {
  const base64 = value.replace(/-/g, '+').replace(/_/g, '/');
  const binary = atob(base64.padEnd(Math.ceil(base64.length / 4) * 4, '='));
  const bytes = new Uint8Array(binary.length);
  for (let index = 0; index < binary.length; index++) {
    bytes[index] = binary.charCodeAt(index);
  }
  return bytes.buffer;
}

function toBase64Url(buffer: ArrayBuffer | null | undefined): string | null {
  if (!buffer) return null;
  let binary = '';
  for (const byte of new Uint8Array(buffer)) {
    binary += String.fromCharCode(byte);
  }
  return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}

function toDescriptors(descriptors: unknown): PublicKeyCredentialDescriptor[] | undefined {
  return (descriptors as CredentialDescriptorJson[] | undefined)?.map(
    (descriptor) => ({ ...descriptor, id: toBuffer(descriptor.id) }) as PublicKeyCredentialDescriptor
  );
}

function parseCreationOptions(options: PasskeyJson): PublicKeyCredentialCreationOptions {
  const api = credentialApi();
  if (api.parseCreationOptionsFromJSON) {
    return api.parseCreationOptionsFromJSON(options);
  }

  const user = options.user as { id: string } & Record<string, unknown>;
  return {
    ...options,
    challenge: toBuffer(options.challenge as string),
    user: { ...user, id: toBuffer(user.id) },
    excludeCredentials: toDescriptors(options.excludeCredentials)
  } as unknown as PublicKeyCredentialCreationOptions;
}

function parseRequestOptions(options: PasskeyJson): PublicKeyCredentialRequestOptions {
  const api = credentialApi();
  if (api.parseRequestOptionsFromJSON) {
    return api.parseRequestOptionsFromJSON(options);
  }

  return {
    ...options,
    challenge: toBuffer(options.challenge as string),
    allowCredentials: toDescriptors(options.allowCredentials)
  } as unknown as PublicKeyCredentialRequestOptions;
}

function serializeCredential(credential: PublicKeyCredential): PasskeyJson {
  const serializable = credential as PublicKeyCredential & { toJSON?: () => PasskeyJson };
  if (typeof serializable.toJSON === 'function') {
    return serializable.toJSON();
  }

  const base = {
    id: credential.id,
    rawId: toBase64Url(credential.rawId),
    type: credential.type,
    authenticatorAttachment: credential.authenticatorAttachment ?? undefined,
    clientExtensionResults: credential.getClientExtensionResults()
  };

  if ('attestationObject' in credential.response) {
    const attestation = credential.response as AuthenticatorAttestationResponse;
    return {
      ...base,
      response: {
        clientDataJSON: toBase64Url(attestation.clientDataJSON),
        attestationObject: toBase64Url(attestation.attestationObject),
        authenticatorData: toBase64Url(attestation.getAuthenticatorData?.()),
        publicKey: toBase64Url(attestation.getPublicKey?.()),
        publicKeyAlgorithm: attestation.getPublicKeyAlgorithm?.(),
        transports: attestation.getTransports?.() ?? []
      }
    };
  }

  const assertion = credential.response as AuthenticatorAssertionResponse;
  return {
    ...base,
    response: {
      clientDataJSON: toBase64Url(assertion.clientDataJSON),
      authenticatorData: toBase64Url(assertion.authenticatorData),
      signature: toBase64Url(assertion.signature),
      userHandle: toBase64Url(assertion.userHandle)
    }
  };
}

export function passkeysSupported(): boolean {
  return (
    typeof window !== 'undefined' &&
    typeof window.PublicKeyCredential === 'function' &&
    !!navigator.credentials
  );
}

export async function passkeyAutofillSupported(): Promise<boolean> {
  if (!passkeysSupported()) return false;
  try {
    return (await credentialApi().isConditionalMediationAvailable?.()) ?? false;
  } catch {
    return false;
  }
}

export async function createPasskey(options: PasskeyJson): Promise<PasskeyJson> {
  const credential = await navigator.credentials.create({ publicKey: parseCreationOptions(options) });
  if (!credential) {
    throw new DOMException('No passkey was created.', 'NotAllowedError');
  }
  return serializeCredential(credential as PublicKeyCredential);
}

export async function getPasskey(
  options: PasskeyJson,
  { autofill = false, signal }: { autofill?: boolean; signal?: AbortSignal } = {}
): Promise<PasskeyJson> {
  const credential = await navigator.credentials.get({
    publicKey: parseRequestOptions(options),
    mediation: autofill ? 'conditional' : undefined,
    signal
  });
  if (!credential) {
    throw new DOMException('No passkey was selected.', 'NotAllowedError');
  }
  return serializeCredential(credential as PublicKeyCredential);
}

export function isPasskeyDismissed(error: unknown): boolean {
  return (
    error instanceof DOMException && (error.name === 'NotAllowedError' || error.name === 'AbortError')
  );
}

export function isPasskeyAlreadyOnDevice(error: unknown): boolean {
  return error instanceof DOMException && error.name === 'InvalidStateError';
}
